//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using Microsoft.Samples.MefShapes.Shapes;
using Microsoft.ComponentModel.Composition.DynamicInstantiation;

namespace Microsoft.Samples.MefShapes
{
    [Export(typeof(IMefShapesGame))]
    public class MefShapesGame : IMefShapesGame
    {
        private readonly Random random = new Random((int)DateTime.Now.Ticks);
        private DispatcherTimer timer;
        private bool isInitialized;

        public MefShapesGame()
        {
            Shapes = new ObservableCollection<IShape>();
            SelectionShapes = new ObservableCollection<PartCreator<IShape, IShapeMetadata>>();
        }

        [Import]
        private IAccelerationStrategy AccelerationStrategy { get; set; }

        [ImportMany(AllowRecomposition = true)]
        public ObservableCollection<PartCreator<IShape, IShapeMetadata>> SelectionShapes { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Import(BoxShapeAttribute.BoxShapeContractName)]
        private IShape EnvironmentShape
        {
            set { Shapes.Add(value); }
        }

        public IShape ActiveShape { get; private set; }

        public ObservableCollection<IShape> Shapes { get; private set; }

        public bool IsRunning { get { return timer != null && timer.IsEnabled; } }

        public void StartGame(Dispatcher dispatcher, int timerTickInterval)
        {
            if (!isInitialized)
            {
                SendNextShape();
                isInitialized = true;
            }

            if (timer == null || !timer.IsEnabled)
            {
                timer = new DispatcherTimer(TimeSpan.FromMilliseconds(timerTickInterval), DispatcherPriority.Normal, TimerTick, dispatcher);
            }
        }

        public void StopGame()
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            ActiveShape.Move(Direction.Down);
        }

        private void SendNextShape()
        {
            int randomIndex = random.Next(this.SelectionShapes.Count);
            IShape shape = this.SelectionShapes[randomIndex].CreatePart().ExportedValue;
            shape.ReachedBottom += shape_ReachedBottom;
            Shapes.Add(shape);
            ActiveShape = shape;
        }


        private void shape_ReachedBottom(object sender, ReachedBottomEventArgs e)
        {
            ((Shape)sender).ReachedBottom -= shape_ReachedBottom;

            if (e.LayersRemoved > 0 && timer != null)
            {
                if (AccelerationStrategy.Acceleration != 0)
                {
                    TimeSpan newInterval = TimeSpan.FromMilliseconds(timer.Interval.TotalMilliseconds / AccelerationStrategy.Acceleration);
                    timer.Interval = newInterval;
                }
            }

            SendNextShape();
        }
    }
}
