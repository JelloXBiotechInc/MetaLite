using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace System.Windows.Controls
{
    /// <summary>
    /// The platform does not currently support relative sized translation values. 
    /// This primitive control walks through visual state animation storyboards
    /// and looks for identifying values to use as percentages.
    /// </summary>
    public class RelativeAnimatingContentControl : ContentControl
    {
        /// <summary>
        /// A simple Epsilon-style value used for trying to determine if a double has an identifying value. 
        /// </summary>
        const double SimpleDoubleComparisonEpsilon = 0.000009;

        double _knownWidth, _knownHeight;

        /// <summary>
        /// A set of custom animation adapters used to update the animation
        /// storyboards when the size of the control changes.
        /// </summary>
        List<AnimationValueAdapter> _specialAnimations;

        public RelativeAnimatingContentControl()
        {
            SizeChanged += (s, e) =>
            {
                if (e != null && e.NewSize.Height > 0 && e.NewSize.Width > 0)
                {
                    _knownWidth = e.NewSize.Width;
                    _knownHeight = e.NewSize.Height;

                    UpdateAnyAnimationValues();
                }
            };
        }

        /// <summary>
        /// Walks through the known storyboards in the control's template that
        /// may contain identifying values, storing them for future
        /// use and updates.
        /// </summary>
        void UpdateAnyAnimationValues()
        {
            if (_knownHeight > 0 && _knownWidth > 0)
            {
                // Initially, before any special animations have been found,
                // the visual state groups of the control must be explored. 
                // By definition they must be at the implementation root of the
                // control.
                if (_specialAnimations == null)
                {
                    _specialAnimations = new List<AnimationValueAdapter>();

                    foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(this))
                    {
                        if (group == null) continue;
                        foreach (VisualState state in group.States)
                        {
                            if (state != null)
                            {
                                Storyboard sb = state.Storyboard;

                                if (sb != null)
                                {
                                    // Examine all children of the storyboards,
                                    // looking for either type of double
                                    // animation.
                                    foreach (Timeline timeline in sb.Children)
                                    {
                                        DoubleAnimation da = timeline as DoubleAnimation;
                                        DoubleAnimationUsingKeyFrames dakeys = timeline as DoubleAnimationUsingKeyFrames;
                                        if (da != null)
                                        {
                                            // Look for a special value in the To property.
                                            if (da.To.HasValue)
                                            {
                                                var d = DoubleAnimationToAdapter.GetDimensionFromIdentifyingValue(da.To.Value);
                                                if (d.HasValue)
                                                    _specialAnimations.Add(new DoubleAnimationToAdapter(d.Value, da));
                                            }

                                            // Look for a special value in the From property.
                                            if (da.From.HasValue)
                                            {
                                                var d = DoubleAnimationFromAdapter.GetDimensionFromIdentifyingValue(da.To.Value);
                                                if (d.HasValue)
                                                    _specialAnimations.Add(new DoubleAnimationFromAdapter(d.Value, da));
                                            }
                                        }
                                        else if (dakeys != null)
                                        {
                                            // Look through all keyframes in the instance.
                                            foreach (DoubleKeyFrame frame in dakeys.KeyFrames)
                                            {
                                                var d = DoubleAnimationFrameAdapter.GetDimensionFromIdentifyingValue(frame.Value);
                                                if (d.HasValue)
                                                    _specialAnimations.Add(new DoubleAnimationFrameAdapter(d.Value, frame));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Update special animation values relative to the current size.
                foreach (AnimationValueAdapter adapter in _specialAnimations)
                    adapter.UpdateWithNewDimension(_knownWidth, _knownHeight);

                // HACK: force storyboard to use new values
                foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(this))
                {
                    if (group == null) continue;
                    foreach (VisualState state in group.States)
                    {
                        if (state != null)
                        {
                            Storyboard sb = state.Storyboard;

                            if (sb != null)
                            {
                                // need to kick the storyboard, otherwise new values are not taken into account.
                                // it's sad, really don't want to start storyboards in vsm, but I see no other option
                                sb.Begin(this);
                            }
                        }
                    }
                }
            }
        }

        #region Animation updating system
        /// <summary>
        /// A selection of dimensions of interest for updating an animation.
        /// </summary>
        enum DoubleAnimationDimension
        {
            /// <summary>
            /// The width (horizontal) dimension.
            /// </summary>
            Width,

            /// <summary>
            /// The height (vertical) dimension.
            /// </summary>
            Height,
        }

        /// <summary>
        /// A simple class designed to store information about a specific 
        /// animation instance and its properties. Able to update the values at
        /// runtime.
        /// </summary>
        abstract class AnimationValueAdapter
        {
            /// <summary>
            /// Gets or sets the original double value.
            /// </summary>
            protected double OriginalValue { get; set; }

            /// <summary>
            /// Initializes a new instance of the AnimationValueAdapter type.
            /// </summary>
            /// <param name="dimension">The dimension of interest for updates.</param>
            public AnimationValueAdapter(DoubleAnimationDimension dimension) { Dimension = dimension; }

            /// <summary>
            /// Gets the dimension of interest for the control.
            /// </summary>
            public DoubleAnimationDimension Dimension { get; private set; }

            /// <summary>
            /// Updates the original instance based on new dimension information
            /// from the control. Takes both and allows the subclass to make the
            /// decision on which ratio, values, and dimension to use.
            /// </summary>
            /// <param name="width">The width of the control.</param>
            /// <param name="height">The height of the control.</param>
            public abstract void UpdateWithNewDimension(double width, double height);
        }

        abstract class GeneralAnimationValueAdapter<T> : AnimationValueAdapter
        {
            /// <summary>
            /// Stores the animation instance.
            /// </summary>
            protected T Instance { get; set; }

            protected abstract double Value { get; set; }

            /// <summary>
            /// Gets the initial value (minus the identifying value portion) that the
            /// designer stored within the visual state animation property.
            /// </summary>
            protected double InitialValue { get; private set; }

            /// <summary>
            /// The ratio based on the original identifying value, used for computing
            /// the updated animation property of interest when the size of the
            /// control changes.
            /// </summary>
            double _ratio;

            /// <summary>
            /// Initializes a new instance of the GeneralAnimationValueAdapter
            /// type.
            /// </summary>
            /// <param name="d">The dimension of interest.</param>
            /// <param name="instance">The animation type instance.</param>
            public GeneralAnimationValueAdapter(DoubleAnimationDimension d, T instance)
                : base(d)
            {
                Instance = instance;

                InitialValue = StripIdentifyingValueOff(Value);
                _ratio = InitialValue / 100;
            }

            /// <summary>
            /// Approximately removes the identifying value from a value.
            /// </summary>
            /// <param name="number">The initial number.</param>
            /// <returns>Returns a double with an adjustment for the identifying
            /// value portion of the number.</returns>
            public double StripIdentifyingValueOff(double number)
            {
                return Dimension == DoubleAnimationDimension.Width ? number - .1 : number - .2;
            }

            /// <summary>
            /// Retrieves the dimension, if any, from the number. If the number
            /// does not have an identifying value, null is returned.
            /// </summary>
            /// <param name="number">The double value.</param>
            /// <returns>Returns a double animation dimension if the number was
            /// contained an identifying value; otherwise, returns null.</returns>
            public static DoubleAnimationDimension? GetDimensionFromIdentifyingValue(double number)
            {
                double floor = Math.Floor(number),
                    remainder = number - floor;

                if (remainder >= .1 - SimpleDoubleComparisonEpsilon && remainder <= .1 + SimpleDoubleComparisonEpsilon)
                    return DoubleAnimationDimension.Width;
                if (remainder >= .2 - SimpleDoubleComparisonEpsilon && remainder <= .2 + SimpleDoubleComparisonEpsilon)
                    return DoubleAnimationDimension.Height;

                return null;
            }

            /// <summary>
            /// Updates the animation instance based on the dimensions of the
            /// control.
            /// </summary>
            /// <param name="width">The width of the control.</param>
            /// <param name="height">The height of the control.</param>
            public override void UpdateWithNewDimension(double width, double height)
            {
                double size = Dimension == DoubleAnimationDimension.Width ? width : height;
                UpdateValue(size);
            }

            /// <summary>
            /// Updates the value of the property.
            /// </summary>
            /// <param name="sizeToUse">The size of interest to use with a ratio
            /// computation.</param>
            void UpdateValue(double sizeToUse) { Value = sizeToUse * _ratio; }
        }

        /// <summary>
        /// Adapter for DoubleAnimation's To property.
        /// </summary>
        class DoubleAnimationToAdapter : GeneralAnimationValueAdapter<DoubleAnimation>
        {
            protected override double Value
            {
                get { return (double)Instance.To; }
                set { Instance.To = value; }
            }

            /// <summary>
            /// Initializes a new instance of the DoubleAnimationToAdapter type.
            /// </summary>
            /// <param name="dimension">The dimension of interest.</param>
            /// <param name="instance">The instance of the animation type.</param>
            public DoubleAnimationToAdapter(DoubleAnimationDimension dimension, DoubleAnimation instance)
                : base(dimension, instance) { }
        }

        /// <summary>
        /// Adapter for DoubleAnimation's From property.
        /// </summary>
        class DoubleAnimationFromAdapter : GeneralAnimationValueAdapter<DoubleAnimation>
        {
            protected override double Value
            {
                get { return (double)Instance.From; }
                set { Instance.From = value; }
            }

            /// <summary>
            /// Initializes a new instance of the DoubleAnimationFromAdapter 
            /// type.
            /// </summary>
            /// <param name="dimension">The dimension of interest.</param>
            /// <param name="instance">The instance of the animation type.</param>
            public DoubleAnimationFromAdapter(DoubleAnimationDimension dimension, DoubleAnimation instance)
                : base(dimension, instance) { }
        }

        /// <summary>
        /// Adapter for double key frames.
        /// </summary>
        class DoubleAnimationFrameAdapter : GeneralAnimationValueAdapter<DoubleKeyFrame>
        {
            protected override double Value
            {
                get { return (double)Instance.Value; }
                set { Instance.Value = value; }
            }

            /// <summary>
            /// Initializes a new instance of the DoubleAnimationFrameAdapter
            /// type.
            /// </summary>
            /// <param name="dimension">The dimension of interest.</param>
            /// <param name="frame">The instance of the animation type.</param>
            public DoubleAnimationFrameAdapter(DoubleAnimationDimension dimension, DoubleKeyFrame frame)
                : base(dimension, frame) { }
        }
        #endregion
    }
}