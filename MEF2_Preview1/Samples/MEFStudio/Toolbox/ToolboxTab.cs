//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.ObjectModel;

namespace ToolboxLibrary
{
	/// <summary>
	/// ToolboxTabs.
	/// </summary>
	public class ToolboxTab
	{
		private string name = null;
        private Collection<ToolboxItem> toolboxItemCollection = null;

		public ToolboxTab()
		{
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public Collection<ToolboxItem> ToolboxItems
		{
			get
			{
				return toolboxItemCollection;
			}
			set
			{
				toolboxItemCollection = value;
			}
		}
	}
}
