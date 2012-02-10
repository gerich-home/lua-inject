//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace MoreDesigners
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.Design;
	using System.ComponentModel.Design.Serialization;
	using System.Collections;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Text;
	using System.Windows.Forms;
	using System.Xml;

    /// <summary>
    /// Inherits from BasicDesignerLoader. It can persist the HostSurface
    /// to an Xml file and can also parse the Xml file to re-create the
    /// RootComponent and all the components that it hosts.
    /// </summary>
	public class BasicHostLoader: BasicDesignerLoader
	{
		private IComponent root;
		private bool dirty = true;
		private bool unsaved;
		private string fileName;
		private IDesignerLoaderHost host;
		private XmlDocument xmlDocument;
		private static readonly Attribute[] propertyAttributes = new Attribute[] {
			DesignOnlyAttribute.No
		};
		private Type rootComponentType;
        private string name;
		
		#region Constructors

		/// Empty constructor simply creates a new form.
		public BasicHostLoader(Type rootComponentType, string name)
		{
			this.rootComponentType = rootComponentType;
            this.name = name;
            this.Modified = true;
		}

        /// <summary>
        /// This constructor takes a file name.  This file
		/// should exist on disk and consist of XML that
		/// can be read by a data set.
        /// </summary>
        public BasicHostLoader(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			this.fileName = fileName;
		}
		#endregion

		#region Overriden methods of BasicDesignerLoader

		// Called by the host when we load a document.
		protected override void PerformLoad(IDesignerSerializationManager designerSerializationManager)
		{
			this.host = this.LoaderHost;

			if (host == null)
			{
				throw new ArgumentNullException("BasicHostLoader.BeginLoad: Invalid designerLoaderHost.");
			}

			// The loader will put error messages in here.
			ArrayList errors = new ArrayList();
			bool successful = true;
			string baseClassName;

			// If no filename was passed in, just create a form and be done with it.  If a file name
			// was passed, read it.
			if (fileName == null)
			{
                baseClassName = name;
				if (rootComponentType == typeof(Form))
				{
					host.CreateComponent(typeof(Form));
				}
				else if (rootComponentType == typeof(UserControl))
				{
					host.CreateComponent(typeof(UserControl));
				}
				else if (rootComponentType == typeof(Component))
				{
					host.CreateComponent(typeof(Component));
				}
				else
				{
					throw new Exception("Undefined Host Type: " + rootComponentType.ToString());
				}
			}
			else
			{
				baseClassName = ReadFile(fileName, errors, out xmlDocument);
			}

			// Now that we are done with the load work, we need to begin to listen to events.
			// Listening to event notifications is how a designer "Loader" can also be used
			// to save data.  If we wanted to integrate this loader with source code control,
			// we would listen to the "ing" events as well as the "ed" events.
			IComponentChangeService cs = host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;

			if (cs != null)
			{
				cs.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);
				cs.ComponentAdded += new ComponentEventHandler(OnComponentAddedRemoved);
				cs.ComponentRemoved += new ComponentEventHandler(OnComponentAddedRemoved);
			}

			// Let the host know we are done loading.
			host.EndLoad(baseClassName, successful, errors);

			// We've just loaded a document, so you can bet we need to flush changes.
			dirty = true;
			unsaved = false;
		}

        /// <summary>
        /// This method is called by the designer host whenever it wants the
		/// designer loader to flush any pending changes.  Flushing changes
		/// does not mean the same thing as saving to disk.  For example,
		/// In Visual Studio, flushing changes causes new code to be generated
		/// and inserted into the text editing window.  The user can edit
		/// the new code in the editing window, but nothing has been saved
		/// to disk.  This sample adheres to this separation between flushing
		/// and saving, since a flush occurs whenever the code windows are
		/// displayed or there is a build. Neither of those items demands a save.
        /// </summary>
        protected override void PerformFlush(IDesignerSerializationManager designerSerializationManager)
		{
			// Nothing to flush if nothing has changed.
			if (!dirty)
			{
				return;
			}

			PerformFlushWorker();
		}
		public override void Dispose()
		{
			// Always remove attached event handlers in Dispose.
			IComponentChangeService cs = host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;

			if (cs != null)
			{
				cs.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);
				cs.ComponentAdded -= new ComponentEventHandler(OnComponentAddedRemoved);
				cs.ComponentRemoved -= new ComponentEventHandler(OnComponentAddedRemoved);
			}
		}
		
		#endregion

		#region Helper methods

        /// <summary>
        /// Simple helper method that returns true if the given type converter supports
		/// two-way conversion of the given type.
        /// </summary>
        private bool GetConversionSupported(TypeConverter converter, Type conversionType)
		{
			return (converter.CanConvertFrom(conversionType) && converter.CanConvertTo(conversionType));
		}

        /// <summary>
        /// As soon as things change, we're dirty, so Flush()ing will give us a new
		/// xmlDocument and codeCompileUnit.
        /// </summary>
        private void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
		{
			dirty = true;
			unsaved = true;
		}

		private void OnComponentAddedRemoved(object sender, ComponentEventArgs ce)
		{
			dirty = true;
			unsaved = true;
		}

        /// <summary>
        /// This method prompts the user to see if it is OK to dispose this document.  
		/// The prompt only happens if the user has made changes.
        /// </summary>
        internal bool PromptDispose()
		{
			if (dirty || unsaved)
			{
				switch (MessageBox.Show("Save changes to existing designer?", "Unsaved Changes", MessageBoxButtons.YesNoCancel))
				{
					case DialogResult.Yes :
						Save(false);
						break;

					case DialogResult.Cancel :
						return false;
				}
			}

			return true;
		}

		#endregion

		#region Serialize - Flush
        /// <summary>
        /// This will recussively go through all the objects in the tree and
        /// serialize them to Xml
        /// </summary>
		public void PerformFlushWorker()
		{
			XmlDocument document = new XmlDocument();
			document.AppendChild(document.CreateElement("DOCUMENT_ELEMENT"));

			IDesignerHost idh = (IDesignerHost)this.host.GetService(typeof(IDesignerHost));
			root = idh.RootComponent;

			Hashtable nametable = new Hashtable(idh.Container.Components.Count);
            IDesignerSerializationManager manager = host.GetService(typeof(IDesignerSerializationManager)) as IDesignerSerializationManager;

			document.DocumentElement.AppendChild(WriteObject(document, nametable, root));
			foreach (IComponent comp in idh.Container.Components)
			{
				if (comp != root && !nametable.ContainsKey(comp))
				{
					document.DocumentElement.AppendChild(WriteObject(document, nametable, comp));
				}
			}

			xmlDocument = document;
		}

		private XmlNode WriteObject(XmlDocument document, IDictionary nametable, object value)
		{
			IDesignerHost idh = (IDesignerHost)this.host.GetService(typeof(IDesignerHost));
			Debug.Assert(value != null, "Should not invoke WriteObject with a null value");

			XmlNode node = document.CreateElement("Object");
			XmlAttribute typeAttr = document.CreateAttribute("type");

			typeAttr.Value = value.GetType().AssemblyQualifiedName;
			node.Attributes.Append(typeAttr);

			IComponent component = value as IComponent;

			if (component != null && component.Site != null && component.Site.Name != null)
			{
				XmlAttribute nameAttr = document.CreateAttribute("name");

				nameAttr.Value = component.Site.Name;
				node.Attributes.Append(nameAttr);
				Debug.Assert(nametable[component] == null, "WriteObject should not be called more than once for the same object.  Use WriteReference instead");
				nametable[value] = component.Site.Name;
			}

			bool isControl = (value is Control);

			if (isControl)
			{
				XmlAttribute childAttr = document.CreateAttribute("children");

				childAttr.Value = "Controls";
				node.Attributes.Append(childAttr);
			}

			if (component != null)
			{
				if (isControl)
				{
					foreach (Control child in ((Control)value).Controls)
					{
						if (child.Site != null && child.Site.Container == idh.Container)
						{
							node.AppendChild(WriteObject(document, nametable, child));
						}
					}
				}// if isControl

				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value, propertyAttributes);

				if (isControl)
				{
					PropertyDescriptor controlProp = properties["Controls"];

					if (controlProp != null)
					{
						PropertyDescriptor[] propArray = new PropertyDescriptor[properties.Count - 1];
						int idx = 0;

						foreach (PropertyDescriptor p in properties)
						{
							if (p != controlProp)
							{
								propArray[idx++] = p;
							}
						}

						properties = new PropertyDescriptorCollection(propArray);
					}
				}

				WriteProperties(document, properties, value, node, "Property");

				EventDescriptorCollection events = TypeDescriptor.GetEvents(value, propertyAttributes);
				IEventBindingService bindings = host.GetService(typeof(IEventBindingService)) as IEventBindingService;

				if (bindings != null)
				{
					properties = bindings.GetEventProperties(events);
					WriteProperties(document, properties, value, node, "Event");
				}
			}
			else
			{
				WriteValue(document, value, node);
			}

			return node;
		}
		private void WriteProperties(XmlDocument document, PropertyDescriptorCollection properties, object value, XmlNode parent, string elementName)
		{
			foreach (PropertyDescriptor prop in properties)
			{
				if (prop.Name == "AutoScaleBaseSize")
				{
					string _DEBUG_ = prop.Name;
				}

				if (prop.ShouldSerializeValue(value))
				{
					string compName = parent.Name;
					XmlNode node = document.CreateElement(elementName);
					XmlAttribute attr = document.CreateAttribute("name");

					attr.Value = prop.Name;
					node.Attributes.Append(attr);

					DesignerSerializationVisibilityAttribute visibility = (DesignerSerializationVisibilityAttribute)prop.Attributes[typeof(DesignerSerializationVisibilityAttribute)];

					switch (visibility.Visibility)
					{
						case DesignerSerializationVisibility.Visible :
							if (!prop.IsReadOnly && WriteValue(document, prop.GetValue(value), node))
							{
								parent.AppendChild(node);
							}

							break;

						case DesignerSerializationVisibility.Content :
							object propValue = prop.GetValue(value);

							if (typeof(IList).IsAssignableFrom(prop.PropertyType))
							{
								WriteCollection(document, (IList)propValue, node);
							}
							else
							{
								PropertyDescriptorCollection props = TypeDescriptor.GetProperties(propValue, propertyAttributes);

								WriteProperties(document, props, propValue, node, elementName);
							}

							if (node.ChildNodes.Count > 0)
							{
								parent.AppendChild(node);
							}

							break;

						default :
							break;
					}
				}
			}
		}
		private XmlNode WriteReference(XmlDocument document, IComponent value)
		{
			IDesignerHost idh = (IDesignerHost)this.host.GetService(typeof(IDesignerHost));

			Debug.Assert(value != null && value.Site != null && value.Site.Container == idh.Container, "Invalid component passed to WriteReference");

			XmlNode node = document.CreateElement("Reference");
			XmlAttribute attr = document.CreateAttribute("name");

			attr.Value = value.Site.Name;
			node.Attributes.Append(attr);
			return node;
		}
		private bool WriteValue(XmlDocument document, object value, XmlNode parent)
		{
			IDesignerHost idh = (IDesignerHost)this.host.GetService(typeof(IDesignerHost));

			// For empty values, we just return.  This creates an empty node.
			if (value == null)
			{
				return true;
			}

			TypeConverter converter = TypeDescriptor.GetConverter(value);

			if (GetConversionSupported(converter, typeof(string)))
			{
				parent.InnerText = (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(string));
			}
			else if (GetConversionSupported(converter, typeof(byte[])))
			{
				byte[] data = (byte[])converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(byte[]));

				parent.AppendChild(WriteBinary(document, data));
			}
			else if (GetConversionSupported(converter, typeof(InstanceDescriptor)))
			{
				InstanceDescriptor id = (InstanceDescriptor)converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(InstanceDescriptor));

				parent.AppendChild(WriteInstanceDescriptor(document, id, value));
			}
			else if (value is IComponent && ((IComponent)value).Site != null && ((IComponent)value).Site.Container == idh.Container)
			{
				parent.AppendChild(WriteReference(document, (IComponent)value));
			}
			else if (value.GetType().IsSerializable)
			{
				BinaryFormatter formatter = new BinaryFormatter();
				MemoryStream stream = new MemoryStream();

				formatter.Serialize(stream, value);

				XmlNode binaryNode = WriteBinary(document, stream.ToArray());

				parent.AppendChild(binaryNode);
			}
			else
			{
				return false;
			}

			return true;
		}

		private void WriteCollection(XmlDocument document, IList list, XmlNode parent)
		{
			foreach (object obj in list)
			{
				XmlNode node = document.CreateElement("Item");
				XmlAttribute typeAttr = document.CreateAttribute("type");

				typeAttr.Value = obj.GetType().AssemblyQualifiedName;
				node.Attributes.Append(typeAttr);
				WriteValue(document, obj, node);
				parent.AppendChild(node);
			}
		}
		private XmlNode WriteBinary(XmlDocument document, byte[] value)
		{
			XmlNode node = document.CreateElement("Binary");

			node.InnerText = Convert.ToBase64String(value);
			return node;
		}
		private XmlNode WriteInstanceDescriptor(XmlDocument document, InstanceDescriptor desc, object value)
		{
			XmlNode node = document.CreateElement("InstanceDescriptor");
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();

			formatter.Serialize(stream, desc.MemberInfo);

			XmlAttribute memberAttr = document.CreateAttribute("member");

			memberAttr.Value = Convert.ToBase64String(stream.ToArray());
			node.Attributes.Append(memberAttr);
			foreach (object arg in desc.Arguments)
			{
				XmlNode argNode = document.CreateElement("Argument");

				if (WriteValue(document, arg, argNode))
				{
					node.AppendChild(argNode);
				}
			}

			if (!desc.IsComplete)
			{
				PropertyDescriptorCollection props = TypeDescriptor.GetProperties(value, propertyAttributes);

				WriteProperties(document, props, value, node, "Property");
			}

			return node;
		}

		#endregion

		#region DeSerialize - Load

        /// <summary>
		/// This method is used to parse the given file.  Before calling this 
		/// method the host member variable must be setup.  This method will
		/// create a data set, read the data set from the XML stored in the
		/// file, and then walk through the data set and create components
		/// stored within it.  The data set is stored in the persistedData
		/// member variable upon return.
		/// 
		/// This method never throws exceptions.  It will set the successful
		/// ref parameter to false when there are catastrophic errors it can't
		/// resolve (like being unable to parse the XML).  All error exceptions
		/// are added to the errors array list, including minor errors.
        /// </summary>
        private string ReadFile(string fileName, ArrayList errors, out XmlDocument document)
		{
			string baseClass = null;

			// Anything unexpected is a fatal error.
			//
			try
			{
				// The main form and items in the component tray will be at the
				// same level, so we have to create a higher super-root in order
				// to construct our XmlDocument.
				StreamReader sr = new StreamReader(fileName);
				string cleandown = sr.ReadToEnd();

				cleandown = "<DOCUMENT_ELEMENT>" + cleandown + "</DOCUMENT_ELEMENT>";

				XmlDocument doc = new XmlDocument();

				doc.LoadXml(cleandown);

				// Now, walk through the document's elements.
				//
				foreach (XmlNode node in doc.DocumentElement.ChildNodes)
				{
					if (baseClass == null)
					{
						baseClass = node.Attributes["name"].Value;
					}

					if (node.Name.Equals("Object"))
					{
						ReadObject(node, errors);
					}
					else
					{
						errors.Add(string.Format("Node type {0} is not allowed here.", node.Name));
					}
				}

				document = doc;
			}
			catch (Exception ex)
			{
				document = null;
				errors.Add(ex);
			}
			return baseClass;
		}

        private void ReadEvent(XmlNode childNode, object instance, ArrayList errors)
        {
            IEventBindingService bindings = host.GetService(typeof(IEventBindingService)) as IEventBindingService;

            if (bindings == null)
            {
                errors.Add("Unable to contact event binding service so we can't bind any events");
                return;
            }

            XmlAttribute nameAttr = childNode.Attributes["name"];

            if (nameAttr == null)
            {
                errors.Add("No event name");
                return;
            }

            XmlAttribute methodAttr = childNode.Attributes["method"];

            if (methodAttr == null || methodAttr.Value == null || methodAttr.Value.Length == 0)
            {
                errors.Add(string.Format("Event {0} has no method bound to it"));
                return;
            }

            EventDescriptor evt = TypeDescriptor.GetEvents(instance)[nameAttr.Value];

            if (evt == null)
            {
                errors.Add(string.Format("Event {0} does not exist on {1}", nameAttr.Value, instance.GetType().FullName));
                return;
            }

            PropertyDescriptor prop = bindings.GetEventProperty(evt);

            Debug.Assert(prop != null, "Bad event binding service");
            try
            {
                prop.SetValue(instance, methodAttr.Value);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
        }

		private object ReadInstanceDescriptor(XmlNode node, ArrayList errors)
		{
			// First, need to deserialize the member
			//
			XmlAttribute memberAttr = node.Attributes["member"];

			if (memberAttr == null)
			{
				errors.Add("No member attribute on instance descriptor");
				return null;
			}

			byte[] data = Convert.FromBase64String(memberAttr.Value);
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream(data);
			MemberInfo mi = (MemberInfo)formatter.Deserialize(stream);
			object[] args = null;

			// Check to see if this member needs arguments.  If so, gather
			// them from the XML.
			if (mi is MethodBase)
			{
				ParameterInfo[] paramInfos = ((MethodBase)mi).GetParameters();

				args = new object[paramInfos.Length];

				int idx = 0;

				foreach (XmlNode child in node.ChildNodes)
				{
					if (child.Name.Equals("Argument"))
					{
						object value;

						if (!ReadValue(child, TypeDescriptor.GetConverter(paramInfos[idx].ParameterType), errors, out value))
						{
							return null;
						}

						args[idx++] = value;
					}
				}

				if (idx != paramInfos.Length)
				{
					errors.Add(string.Format("Member {0} requires {1} arguments, not {2}.", mi.Name, args.Length, idx));
					return null;
				}
			}

			InstanceDescriptor id = new InstanceDescriptor(mi, args);
			object instance = id.Invoke();

			// Ok, we have our object.  Now, check to see if there are any properties, and if there are, 
			// set them.
			//
			foreach (XmlNode prop in node.ChildNodes)
			{
				if (prop.Name.Equals("Property"))
				{
					ReadProperty(prop, instance, errors);
				}
			}

			return instance;
		}
		/// Reads the "Object" tags. This returns an instance of the
		/// newly created object. Returns null if there was an error.
		private object ReadObject(XmlNode node, ArrayList errors)
		{
			XmlAttribute typeAttr = node.Attributes["type"];

			if (typeAttr == null)
			{
				errors.Add("<Object> tag is missing required type attribute");
				return null;
			}

			Type type = Type.GetType(typeAttr.Value);

			if (type == null)
			{
				errors.Add(string.Format("Type {0} could not be loaded.", typeAttr.Value));
				return null;
			}

			// This can be null if there is no name for the object.
			//
			XmlAttribute nameAttr = node.Attributes["name"];
			object instance;

			if (typeof(IComponent).IsAssignableFrom(type))
			{
				if (nameAttr == null)
				{
					instance = host.CreateComponent(type);
				}
				else
				{
					instance = host.CreateComponent(type, nameAttr.Value);
				}
			}
			else
			{
				instance = Activator.CreateInstance(type);
			}

			// Got an object, now we must process it.  Check to see if this tag
			// offers a child collection for us to add children to.
			//
			XmlAttribute childAttr = node.Attributes["children"];
			IList childList = null;

			if (childAttr != null)
			{
				PropertyDescriptor childProp = TypeDescriptor.GetProperties(instance)[childAttr.Value];

				if (childProp == null)
				{
					errors.Add(string.Format("The children attribute lists {0} as the child collection but this is not a property on {1}", childAttr.Value, instance.GetType().FullName));
				}
				else
				{
					childList = childProp.GetValue(instance) as IList;
					if (childList == null)
					{
						errors.Add(string.Format("The property {0} was found but did not return a valid IList", childProp.Name));
					}
				}
			}

			// Now, walk the rest of the tags under this element.
			//
			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name.Equals("Object"))
				{
					// Another object.  In this case, create the object, and
					// parent it to ours using the children property.  If there
					// is no children property, bail out now.
					if (childAttr == null)
					{
						errors.Add("Child object found but there is no children attribute");
						continue;
					}

					// no sense doing this if there was an error getting the property.  We've already reported the
					// error above.
					if (childList != null)
					{
						object childInstance = ReadObject(childNode, errors);

						childList.Add(childInstance);
					}
				}
				else if (childNode.Name.Equals("Property"))
				{
					// A property.  Ask the property to parse itself.
					//
					ReadProperty(childNode, instance, errors);
				}
				else if (childNode.Name.Equals("Event"))
				{
					// An event.  Ask the event to parse itself.
					//
					ReadEvent(childNode, instance, errors);
				}
			}

			return instance;
		}
		/// Parses the given XML node and sets the resulting property value.
		private void ReadProperty(XmlNode node, object instance, ArrayList errors)
		{
			XmlAttribute nameAttr = node.Attributes["name"];

			if (nameAttr == null)
			{
				errors.Add("Property has no name");
				return;
			}

			PropertyDescriptor prop = TypeDescriptor.GetProperties(instance)[nameAttr.Value];

			if (prop == null)
			{
				errors.Add(string.Format("Property {0} does not exist on {1}", nameAttr.Value, instance.GetType().FullName));
				return;
			}

			// Get the type of this property.  We have three options:
			// 1.  A normal read/write property.
			// 2.  A "Content" property.
			// 3.  A collection.
			//
			bool isContent = prop.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content);

			if (isContent)
			{
				object value = prop.GetValue(instance);

				// Handle the case of a content property that is a collection.
				//
				if (value is IList)
				{
					foreach (XmlNode child in node.ChildNodes)
					{
						if (child.Name.Equals("Item"))
						{
							object item;
							XmlAttribute typeAttr = child.Attributes["type"];

							if (typeAttr == null)
							{
								errors.Add("Item has no type attribute");
								continue;
							}

							Type type = Type.GetType(typeAttr.Value);

							if (type == null)
							{
								errors.Add(string.Format("Item type {0} could not be found.", typeAttr.Value));
								continue;
							}

							if (ReadValue(child, TypeDescriptor.GetConverter(type), errors, out item))
							{
								try
								{
									((IList)value).Add(item);
								}
								catch (Exception ex)
								{
									errors.Add(ex.Message);
								}
							}
						}
						else
						{
							errors.Add(string.Format("Only Item elements are allowed in collections, not {0} elements.", child.Name));
						}
					}
				}
				else
				{
					// Handle the case of a content property that consists of child properties.
					//
					foreach (XmlNode child in node.ChildNodes)
					{
						if (child.Name.Equals("Property"))
						{
							ReadProperty(child, value, errors);
						}
						else
						{
							errors.Add(string.Format("Only Property elements are allowed in content properties, not {0} elements.", child.Name));
						}
					}
				}
			}
			else
			{
				object value;

				if (ReadValue(node, prop.Converter, errors, out value))
				{
					// ReadValue succeeded.  Fill in the property value.
					//
					try
					{
						prop.SetValue(instance, value);
					}
					catch (Exception ex)
					{
						errors.Add(ex.Message);
					}
				}
			}
		}
		/// Generic function to read an object value.  Returns true if the read
		/// succeeded.
		private bool ReadValue(XmlNode node, TypeConverter converter, ArrayList errors, out object value)
		{
			try
			{
				foreach (XmlNode child in node.ChildNodes)
				{
					if (child.NodeType == XmlNodeType.Text)
					{
						value = converter.ConvertFromInvariantString(node.InnerText);
						return true;
					}
					else if (child.Name.Equals("Binary"))
					{
						byte[] data = Convert.FromBase64String(child.InnerText);

						// Binary blob.  Now, check to see if the type converter
						// can convert it.  If not, use serialization.
						//
						if (GetConversionSupported(converter, typeof(byte[])))
						{
							value = converter.ConvertFrom(null, CultureInfo.InvariantCulture, data);
							return true;
						}
						else
						{
							BinaryFormatter formatter = new BinaryFormatter();
							MemoryStream stream = new MemoryStream(data);

							value = formatter.Deserialize(stream);
							return true;
						}
					}
					else if (child.Name.Equals("InstanceDescriptor"))
					{
						value = ReadInstanceDescriptor(child, errors);
						return (value != null);
					}
					else
					{
						errors.Add(string.Format("Unexpected element type {0}", child.Name));
						value = null;
						return false;
					}
				}

				// If we get here, it is because there were no nodes.  No nodes and no inner
				// text is how we signify null.
				//
				value = null;
				return true;
			}
			catch (Exception ex)
			{
				errors.Add(ex.Message);
				value = null;
				return false;
			}
		}

		#endregion

		#region Public methods

		/// This method writes out the contents of our designer in XML.
		/// It writes out the contents of xmlDocument.
		public string GetCode()
		{
			Flush();
			
			StringWriter sw;
			sw = new StringWriter();

			XmlTextWriter xtw = new XmlTextWriter(sw);

			xtw.Formatting = Formatting.Indented;
			xmlDocument.WriteTo(xtw);

			string cleanup = sw.ToString().Replace("<DOCUMENT_ELEMENT>", "");

			cleanup = cleanup.Replace("</DOCUMENT_ELEMENT>", "");
			sw.Close();
			return cleanup;
		}

		public void Save()
		{
			Save(false);
		}

        /// <summary>
		/// Save the current state of the loader. If the user loaded the file
		/// or saved once before, then he doesn't need to select a file again.
		/// Unless this is being called as a result of "Save As..." being clicked,
		/// in which case forceFilePrompt will be true.
        /// </summary>
        public void Save(bool forceFilePrompt)
		{
			try
			{
				Flush();

				int filterIndex = 3;

				if ((fileName == null) || forceFilePrompt)
				{
					SaveFileDialog dlg = new SaveFileDialog();

					dlg.DefaultExt = "xml";
					dlg.Filter = "XML Files|*.xml";
					if (dlg.ShowDialog() == DialogResult.OK)
					{
						fileName = dlg.FileName;
						filterIndex = dlg.FilterIndex;
					}
				}

				if (fileName != null)
				{
					switch (filterIndex)
					{
						case 1 :
							{
								// Write out our xmlDocument to a file.
								StringWriter sw = new StringWriter();
								XmlTextWriter xtw = new XmlTextWriter(sw);

								xtw.Formatting = Formatting.Indented;
								xmlDocument.WriteTo(xtw);

								// Get rid of our artificial super-root before we save out
								// the XML.
								//
								string cleanup = sw.ToString().Replace("<DOCUMENT_ELEMENT>", "");

								cleanup = cleanup.Replace("</DOCUMENT_ELEMENT>", "");
								xtw.Close();

								StreamWriter file = new StreamWriter(fileName);

								file.Write(cleanup);
								file.Close();
							}
							break;
					}
					unsaved = false;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error during save: " + ex.ToString());
			}
		}

		#endregion

	}// class
}// namespace

