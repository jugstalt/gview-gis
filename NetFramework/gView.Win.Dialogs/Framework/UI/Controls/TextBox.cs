using System;
using System.Globalization; 
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms.Design;

[assembly:CLSCompliant(true)] 
namespace gView.Framework.UI.Controls
{
	[Browsable(false)]
	[Designer(typeof(AdvancedTextBox.Designer))]	
	public abstract class AdvancedTextBox : System.Windows.Forms.TextBox
	{
		/// <summary> The Behavior object associated with this TextBox. </summary>
		protected Behavior m_behavior = null; 	// must be initialized by derived classes
		
		/// <summary>
		///   Initializes a new instance of the TextBox class. </summary>
		/// <remarks>
		///   This constructor is just for convenience for derived classes.  It does nothing. </remarks>
		protected AdvancedTextBox()
		{
		}

		/// <summary>
		///   Initializes a new instance of the TextBox class by explicitly assigning its Behavior field. </summary>
		/// <param name="behavior">
		///   The <see cref="Behavior" /> object to associate the textbox with. </param>
		/// <remarks>
		///   This constructor provides a way for derived classes to set the internal Behavior object
		///   to something other than the default value (such as <c>null</c>). </remarks>
		protected AdvancedTextBox(Behavior behavior)
		{
			m_behavior = behavior;
		}

		/// <summary>
		///   Checks if the textbox's text is valid and if not updates it with a valid value. </summary>
		/// <returns>
		///   If the textbox's text is updated (because it wasn't valid), the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   This method delegates to <see cref="Behavior.UpdateText">Behavior.UpdateText</see>. </remarks>
		public bool UpdateText()
		{			
			return m_behavior.UpdateText();
		}		

		/// <summary>
		///   Gets or sets the flags associated with self's Behavior. </summary>
		/// <remarks>
		///   This property delegates to <see cref="Behavior.Flags">Behavior.Flags</see>. </remarks>
		/// <seealso cref="ModifyFlags" />
		[Category("Behavior")]
		[Description("The flags (on/off attributes) associated with the Behavior.")]
		public int Flags
		{
			get 
			{ 
				return m_behavior.Flags; 
			}
			set
			{
				m_behavior.Flags = value;
			}
		}
		
		/// <summary>
		///   Adds or removes flags from self's Behavior. </summary>
		/// <param name="flags">
		///   The bits to be turned on (ORed) or turned off in the internal flags member. </param>
		/// <param name="addOrRemove">
		///   If true the flags are added, otherwise they're removed. </param>
		/// <remarks>
		///   This method delegates to <see cref="Behavior.ModifyFlags">Behavior.ModifyFlags</see>. </remarks>
		/// <seealso cref="Flags" />
		public void ModifyFlags(int flags, bool addOrRemove)
		{
			m_behavior.ModifyFlags(flags, addOrRemove);
		}

		/// <summary>
		///   Checks if the textbox's value is valid and if not proceeds according to the behavior's <see cref="Flags" />. </summary>
		/// <returns>
		///   If the validation succeeds, the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   This method delegates to <see cref="Behavior.Validate">Behavior.Validate</see>. </remarks>
		/// <seealso cref="IsValid" />
		public bool Validate()
		{
			return m_behavior.Validate();
		}

		/// <summary>
		///   Checks if the textbox contains a valid value. </summary>
		/// <returns>
		///   If the value is valid, the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   This method delegates to <see cref="Behavior.IsValid">Behavior.IsValid</see>. </remarks>
		/// <seealso cref="Validate" />
		public bool IsValid()
		{
			return m_behavior.IsValid();
		}

		/// <summary>
		///   Show an error message box. </summary>
		/// <param name="message">
		///   The message to show. </param>
		/// <remarks>
		///   This property delegates to <see cref="Behavior.ShowErrorMessageBox">Behavior.ShowErrorMessageBox</see>. </remarks>
		/// <seealso cref="ShowErrorIcon" />
		/// <seealso cref="ErrorMessage" />
		public void ShowErrorMessageBox(string message)
		{
			m_behavior.ShowErrorMessageBox(message);
		}

		/// <summary>
		///   Show a blinking icon next to the textbox with an error message. </summary>
		/// <param name="message">
		///   The message to show when the cursor is placed over the icon. </param>
		/// <remarks>
		///   This property delegates to <see cref="Behavior.ShowErrorIcon">Behavior.ShowErrorIcon</see>. </remarks>
		/// <seealso cref="ShowErrorMessageBox" />
		/// <seealso cref="ErrorMessage" />
		public void ShowErrorIcon(string message)
		{
			m_behavior.ShowErrorIcon(message);
		}

		/// <summary>
		///   Gets the error message used to notify the user to enter a valid value. </summary>
		/// <remarks>
		///   This property delegates to <see cref="Behavior.ErrorMessage">Behavior.ErrorMessage</see>. </remarks>
		/// <seealso cref="Validate" />
		/// <seealso cref="IsValid" />
		[Browsable(false)]
		public string ErrorMessage
		{
			get
			{
				return m_behavior.ErrorMessage;
			}
		}
		/// <summary>
		///   Designer class used to prevent the Text property from being set to
		///   some default value (ie. textBox1) and to remove any properties the designer 
		///   should not generate code for. </summary>
		internal class Designer : ControlDesigner 
		{
			/// <summary>
			///   This typically sets the control's Text property.  
			///   Here it does nothing so the Text is left blank. </summary>
			public override void OnSetComponentDefaults()
			{
			}
		}
	}
	
	public class AlphanumericTextBox : AdvancedTextBox
	{
		public AlphanumericTextBox()
		{
			m_behavior = new AlphanumericBehavior(this);
		}
	
		public AlphanumericTextBox(char[] invalidChars)
		{
			m_behavior = new AlphanumericBehavior(this, invalidChars);
		}	

		public AlphanumericTextBox(string invalidChars)
		{
			m_behavior = new AlphanumericBehavior(this, invalidChars);
		}	
	
		public AlphanumericTextBox(AlphanumericBehavior behavior) :
			base(behavior)
		{
		}
		
		[Browsable(false)]
		public AlphanumericBehavior Behavior
		{
			get 
			{ 
				return (AlphanumericBehavior)m_behavior; 
			}
		}		
		
		[Category("Behavior")]
		public char[] InvalidChars
		{
			get 
			{ 
				return Behavior.InvalidChars; 
			}
			set 
			{ 
				Behavior.InvalidChars = value;
			}
		}
	}

	public class MaskedTextBox : AdvancedTextBox
	{
		public MaskedTextBox()
		{
			m_behavior = new MaskedBehavior(this);
		}
	
		public MaskedTextBox(string mask)
		{
			m_behavior = new MaskedBehavior(this, mask);
		}	
	
		public MaskedTextBox(MaskedBehavior behavior) :
			base(behavior)
		{
		}
		
		[Browsable(false)]
		public MaskedBehavior Behavior
		{
			get 
			{ 
				return (MaskedBehavior)m_behavior; 
			}
		}			

		[Category("Behavior")]
		[Description("The string used for formatting the characters entered into the textbox. (# = digit)")]
		public string Mask
		{
			get 
			{ 
				return Behavior.Mask; 
			}
			set 
			{ 
				Behavior.Mask = value;
			}
		}
		
		[Browsable(false)]
		public ArrayList Symbols
		{
			get 
			{ 
				return Behavior.Symbols; 
			}
		}

		[Browsable(false)]
		public string NumericText
		{
			get 
			{ 
				return Behavior.NumericText;
			}					
		}		
	}

    [Designer(typeof(NumericTextBox.Designer))]
    public class NumericTextBox : AdvancedTextBox
    {
        public NumericTextBox()
        {
            m_behavior = new NumericBehavior(this);
        }

        public NumericTextBox(int maxWholeDigits, int maxDecimalPlaces)
        {
            m_behavior = new NumericBehavior(this, maxWholeDigits, maxDecimalPlaces);
        }

        public NumericTextBox(NumericBehavior behavior)
            :
            base(behavior)
        {
        }

        [Browsable(false)]
        public NumericBehavior Behavior
        {
            get
            {
                return (NumericBehavior)m_behavior;
            }
        }

        [Browsable(false)]
        public double Double
        {
            get
            {
                try
                {
                    return Convert.ToDouble(Behavior.NumericText);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                Text = value.ToString();
            }
        }

        [Browsable(false)]
        public int Int
        {
            get
            {
                try
                {
                    return Convert.ToInt32(Behavior.NumericText);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                Text = value.ToString();
            }
        }

        [Browsable(false)]
        public long Long
        {
            get
            {
                try
                {
                    return Convert.ToInt64(Behavior.NumericText);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                Text = value.ToString();
            }
        }

        [Browsable(false)]
        public string NumericText
        {
            get
            {
                return Behavior.NumericText;
            }
        }

        [Browsable(false)]
        public string RealNumericText
        {
            get
            {
                return Behavior.RealNumericText;
            }
        }

        [Category("Behavior")]
        public int MaxWholeDigits
        {
            get
            {
                return Behavior.MaxWholeDigits;
            }
            set
            {
                Behavior.MaxWholeDigits = value;
            }
        }

        [Category("Behavior")]
        public int MaxDecimalPlaces
        {
            get
            {
                return Behavior.MaxDecimalPlaces;
            }
            set
            {
                Behavior.MaxDecimalPlaces = value;
            }
        }

        [Category("Behavior")]
        public bool AllowNegative
        {
            get
            {
                return Behavior.AllowNegative;
            }
            set
            {
                Behavior.AllowNegative = value;
            }
        }

        [Category("Behavior")]
        public int DigitsInGroup
        {
            get
            {
                return Behavior.DigitsInGroup;
            }
            set
            {
                Behavior.DigitsInGroup = value;
            }
        }

        [Browsable(false)]
        public char DecimalPoint
        {
            get
            {
                return Behavior.DecimalPoint;
            }
            set
            {
                Behavior.DecimalPoint = value;
            }
        }

        [Browsable(false)]
        public char GroupSeparator
        {
            get
            {
                return Behavior.GroupSeparator;
            }
            set
            {
                Behavior.GroupSeparator = value;
            }
        }

        [Browsable(false)]
        public char NegativeSign
        {
            get
            {
                return Behavior.NegativeSign;
            }
            set
            {
                Behavior.NegativeSign = value;
            }
        }

        [Category("Behavior")]
        public String Prefix
        {
            get
            {
                return Behavior.Prefix;
            }
            set
            {
                Behavior.Prefix = value;
            }
        }

        [Category("Behavior")]
        public double RangeMin
        {
            get
            {
                return Behavior.RangeMin;
            }
            set
            {
                Behavior.RangeMin = value;
            }
        }

        [Category("Behavior")]
        public double RangeMax
        {
            get
            {
                return Behavior.RangeMax;
            }
            set
            {
                Behavior.RangeMax = value;
            }
        }

        private NumericDataType _dataType=NumericDataType.doubleType;
        [Category("Behavior")]
        public NumericDataType DataType
        {
            get { return _dataType; }
            set
            {
                switch (value)
                {
                    case NumericDataType.byteType:
                        this.RangeMax = byte.MaxValue;
                        this.RangeMin = byte.MinValue;
                        this.MaxWholeDigits = 3;
                        this.MaxDecimalPlaces = 0;
                        break;
                    case NumericDataType.shortType:
                        this.RangeMax = short.MaxValue;
                        this.RangeMin = short.MinValue;
                        this.MaxWholeDigits = 5;
                        this.MaxDecimalPlaces = 0;
                        break;
                    case NumericDataType.intType:
                        this.RangeMax = int.MaxValue;
                        this.RangeMin = int.MinValue;
                        this.MaxWholeDigits = 10;
                        this.MaxDecimalPlaces = 0;
                        break;
                    case NumericDataType.longType:
                        this.RangeMax = long.MaxValue;
                        this.RangeMin = long.MinValue;
                        this.MaxWholeDigits = 20;
                        this.MaxDecimalPlaces = 0;
                        break;
                    case NumericDataType.floatType:
                        this.RangeMax = float.MaxValue;
                        this.RangeMin = float.MinValue;
                        this.MaxWholeDigits = 20;
                        this.MaxDecimalPlaces = 5;
                        break;
                    case NumericDataType.doubleType:
                        this.RangeMax = double.MaxValue;
                        this.RangeMin = double.MinValue;
                        this.MaxWholeDigits = 20;
                        this.MaxDecimalPlaces = 5;
                        break;
                }
                _dataType = value;
            }
        }

        internal new class Designer : AdvancedTextBox.Designer
        {
            protected override void PostFilterProperties(IDictionary properties)
            {
                properties.Remove("DecimalPoint");
                properties.Remove("GroupSeparator");
                properties.Remove("NegativeSign");
                properties.Remove("Double");
                properties.Remove("Int");
                properties.Remove("Long");

                base.PostFilterProperties(properties);
            }
        }

        public enum NumericDataType { byteType = 0, shortType = 1, intType = 2, longType = 3, floatType = 4, doubleType = 5 }
    }


	public class IntegerTextBox : NumericTextBox
	{
		public IntegerTextBox() :
			base(null)
		{
			m_behavior = new IntegerBehavior(this);
		}
	
		public IntegerTextBox(int maxWholeDigits) :
			base(null)
		{
			m_behavior = new IntegerBehavior(this, maxWholeDigits);
		}	

		public IntegerTextBox(IntegerBehavior behavior) :
			base(behavior)
		{
		}
	}


	[Designer(typeof(CurrencyTextBox.Designer))]	
	public class CurrencyTextBox : NumericTextBox
	{
		public CurrencyTextBox() :
			base(null)
		{
			m_behavior = new CurrencyBehavior(this);
		}	

		public CurrencyTextBox(CurrencyBehavior behavior) :
			base(behavior)
		{
		}

		internal new class Designer : NumericTextBox.Designer 
		{
			protected override void PostFilterProperties(IDictionary properties)
			{
				properties.Remove("DigitsInGroup");
				properties.Remove("Prefix");
				properties.Remove("MaxDecimalPlaces");

				base.PostFilterProperties(properties);
			}
		}
	}


	[Designer(typeof(DateTextBox.Designer))]	
	public class DateTextBox : AdvancedTextBox
	{
		public DateTextBox()
		{
			m_behavior = new DateBehavior(this);
		}
	
		public DateTextBox(DateBehavior behavior) :
			base(behavior)
		{
		}

		[Browsable(false)]
		public DateBehavior Behavior
		{
			get 
			{ 
				return (DateBehavior)m_behavior; 
			}
		}
		
		[Browsable(false)]
		public int Month
		{
			get 
			{
				return Behavior.Month;
			}
			set
			{					
				Behavior.Month = value;
			}
		}

		[Browsable(false)]
		public int Day
		{
			get 
			{
				return Behavior.Day;
			}
			set
			{					
				Behavior.Day = value;
			}
		}
		
		[Browsable(false)]
		public int Year
		{
			get 
			{
				return Behavior.Year;
			}
			set
			{					
				Behavior.Year = value;
			}
		}

		[Browsable(false)]
		public object Value
		{
			get 
			{
				return Behavior.Value;
			}
			set
			{					
				Behavior.Value = value;
			}
		}

		[Category("Behavior")]
		public DateTime RangeMin
		{
			get 
			{
				return Behavior.RangeMin;
			}
			set
			{					
				Behavior.RangeMin = value;
			}
		}

		[Category("Behavior")]
		public DateTime RangeMax
		{
			get 
			{
				return Behavior.RangeMax;
			}
			set
			{					
				Behavior.RangeMax = value;
			}
		}

		[Browsable(false)]
		public char Separator
		{
			get 
			{
				return Behavior.Separator;
			}
			set
			{					
				Behavior.Separator = value;
			}
		}

		[Browsable(false)]
		public bool ShowDayBeforeMonth
		{
			get 
			{
				return Behavior.ShowDayBeforeMonth;
			}
			set
			{					
				Behavior.ShowDayBeforeMonth = value;
			}
		}

		public void SetDate(int year, int month, int day)
		{
			Behavior.SetDate(year, month, day);
		}		

		internal new class Designer : AdvancedTextBox.Designer 
		{
			protected override void PostFilterProperties(IDictionary properties)
			{
				properties.Remove("Month");
				properties.Remove("Day");
				properties.Remove("Year");
				properties.Remove("Value");
				properties.Remove("Separator");
				properties.Remove("ShowDayBeforeMonth");

				base.PostFilterProperties(properties);
			}
		}
	}


	[Designer(typeof(TimeTextBox.Designer))]	
	public class TimeTextBox : AdvancedTextBox
	{
		public TimeTextBox()
		{
			m_behavior = new TimeBehavior(this);
		}
	
		public TimeTextBox(TimeBehavior behavior) :
			base(behavior)
		{
		}

		[Browsable(false)]
		public TimeBehavior Behavior
		{
			get 
			{ 
				return (TimeBehavior)m_behavior; 
			}
		}			
	
		[Browsable(false)]
		public int Hour
		{
			get
			{			
				return Behavior.Hour;
			}			
			set 
			{
				Behavior.Hour = value;
			}
		}

		[Browsable(false)]
		public int Minute
		{
			get
			{			
				return Behavior.Minute;
			}			
			set 
			{
				Behavior.Minute = value;
			}
		}

		[Browsable(false)]
		public int Second
		{
			get
			{			
				return Behavior.Second;
			}			
			set 
			{
				Behavior.Second = value;
			}
		}

		[Browsable(false)]
		public string AMPM
		{
			get
			{			
				return Behavior.AMPM;
			}			
		}

		[Browsable(false)]
		public object Value
		{
			get
			{			
				return Behavior.Value;
			}			
			set 
			{
				Behavior.Value = value;
			}
		}

		[Category("Behavior")]
		public DateTime RangeMin
		{
			get
			{			
				return Behavior.RangeMin;
			}			
			set 
			{
				Behavior.RangeMin = value;
			}
		}

		[Category("Behavior")]
		public DateTime RangeMax
		{
			get
			{			
				return Behavior.RangeMax;
			}			
			set 
			{
				Behavior.RangeMax = value;
			}
		}

		[Browsable(false)]
		public char Separator
		{
			get
			{			
				return Behavior.Separator;
			}			
			set 
			{
				Behavior.Separator = value;
			}
		}

		[Browsable(false)]
		public bool Show24HourFormat
		{
			get
			{			
				return Behavior.Show24HourFormat;
			}			
			set 
			{
				Behavior.Show24HourFormat = value;
			}
		}

		[Category("Behavior")]
		public bool ShowSeconds
		{
			get
			{			
				return Behavior.ShowSeconds;
			}			
			set 
			{
				Behavior.ShowSeconds = value;
			}
		}

		public void SetTime(int hour, int minute, int second)
		{
			Behavior.SetTime(hour, minute, second);
		}
		
		public void SetTime(int hour, int minute)
		{
			Behavior.SetTime(hour, minute);
		}		

		internal new class Designer : AdvancedTextBox.Designer 
		{
			protected override void PostFilterProperties(IDictionary properties)
			{
				properties.Remove("Hour");
				properties.Remove("Minute");
				properties.Remove("Second");
				properties.Remove("Value");
				properties.Remove("Separator");
				properties.Remove("Show24HourFormat");

				base.PostFilterProperties(properties);
			}
		}
	}


	[Designer(typeof(DateTimeTextBox.Designer))]	
	public class DateTimeTextBox : TimeTextBox
	{
		public DateTimeTextBox() :
			base(null)
		{
			m_behavior = new DateTimeBehavior(this);
		}
	
		public DateTimeTextBox(DateTimeBehavior behavior) :
			base(behavior)
		{
		}
		
		[Browsable(false)]
		public new DateTimeBehavior Behavior
		{
			get 
			{ 
				return (DateTimeBehavior)m_behavior; 
			}
		}			

		[Browsable(false)]
		public int Month
		{
			get 
			{
				return Behavior.Month;
			}
			set
			{					
				Behavior.Month = value;
			}
		}

		[Browsable(false)]
		public int Day
		{
			get 
			{
				return Behavior.Day;
			}
			set
			{					
				Behavior.Day = value;
			}
		}
		
		[Browsable(false)]
		public int Year
		{
			get 
			{
				return Behavior.Year;
			}
			set
			{					
				Behavior.Year = value;
			}
		}

		[Browsable(false)]
		public new object Value
		{
			get 
			{
				return Behavior.Value;
			}
			set
			{					
				Behavior.Value = value;
			}
		}

		[Category("Behavior")]
		public new DateTime RangeMin
		{
			get 
			{
				return Behavior.RangeMin;
			}
			set
			{					
				Behavior.RangeMin = value;
			}
		}

		[Category("Behavior")]
		public new DateTime RangeMax
		{
			get 
			{
				return Behavior.RangeMax;
			}
			set
			{					
				Behavior.RangeMax = value;
			}
		}

		[Browsable(false)]
		public char DateSeparator
		{
			get 
			{ 
				return Behavior.DateSeparator; 
			}
			set 
			{ 
				Behavior.DateSeparator = value; 
			}
		}

		[Browsable(false)]
		public char TimeSeparator
		{
			get 
			{ 
				return Behavior.TimeSeparator; 
			}
			set 
			{ 
				Behavior.TimeSeparator = value; 
			}
		}

		[Browsable(false)]
		private new char Separator
		{
			get
			{
				return Behavior.Separator; 
			}
		}

		[Browsable(false)]
		public bool ShowDayBeforeMonth
		{
			get 
			{
				return Behavior.ShowDayBeforeMonth;
			}
			set
			{					
				Behavior.ShowDayBeforeMonth = value;
			}
		}

		public void SetDate(int year, int month, int day)
		{
			Behavior.SetDate(year, month, day);
		}		

		public void SetDateTime(int year, int month, int day, int hour, int minute)
		{
			Behavior.SetDateTime(year, month, day, hour, minute);
		}

		public void SetDateTime(int year, int month, int day, int hour, int minute, int second)
		{
			Behavior.SetDateTime(year, month, day, hour, minute, second);
		}

		internal new class Designer : TimeTextBox.Designer 
		{
			protected override void PostFilterProperties(IDictionary properties)
			{
				properties.Remove("Month");
				properties.Remove("Day");
				properties.Remove("Year");
				properties.Remove("DateSeparator");
				properties.Remove("TimeSeparator");
				properties.Remove("ShowDayBeforeMonth");

				base.PostFilterProperties(properties);
			}
		}
	}


	public class MultiMaskedTextBox : AdvancedTextBox
	{
		// Fields
		private string m_mask = "";

		/// <summary>
		///   Initializes a new instance of the MultiMaskedTextBox class by setting its mask to an
		///   empty string and setting its Behavior field to <see cref="AlphanumericBehavior">Alphanumeric</see>. </summary>
		public MultiMaskedTextBox() : 
			this("")
		{
		}

		public MultiMaskedTextBox(string mask)
		{
			Mask = mask;
		}

		[Browsable(false)]
		public Behavior Behavior
		{
			get 
			{ 
				return m_behavior; 
			}
		}			

		[Category("Behavior")]
		public string Mask
		{
			get 
			{ 
				return m_mask; 
			}
			set 
			{ 
				if (m_mask == value && m_behavior != null)
					return;
				
				if (m_behavior != null)
					m_behavior.Dispose();
				Text = "";
				
				m_mask = value; 
				int length = value.Length;

				// If it doesn't have numeric place holders then it's alphanumeric
				int position = value.IndexOf('#');
				if (position < 0)
				{
					m_behavior = new AlphanumericBehavior(this, "");
					return;
				}

				// If it's exactly like the date mask, then it's a date
				if (value == "##/##/#### ##:##:##")
				{
					m_behavior = new DateTimeBehavior(this);
					((DateTimeBehavior)m_behavior).ShowSeconds = true;
					return;
				}

				// If it's exactly like the date mask, then it's a date
				else if (value == "##/##/#### ##:##")
				{
					m_behavior = new DateTimeBehavior(this);
					return;
				}

				// If it's exactly like the date mask, then it's a date
				else if (value == "##/##/####")
				{
					m_behavior = new DateBehavior(this);
					return;
				}

				// If it's exactly like the time mask with seconds, then it's a time
				else if (value == "##:##:##")
				{
					m_behavior = new TimeBehavior(this);
					((TimeBehavior)m_behavior).ShowSeconds = true;
					return;
				}

				// If it's exactly like the time mask, then it's a time
				else if (value == "##:##")
				{
					m_behavior = new TimeBehavior(this);
					return;
				}

				// If after the first numeric placeholder, we don't find any foreign characters,
				// then it's numeric, otherwise it's masked numeric.
				string smallMask = value.Substring(position + 1);
				int smallLength = smallMask.Length;
				
				char decimalPoint = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];
				char groupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator[0];

				for (int iPos = 0; iPos < smallLength; iPos++)
				{
					char c = smallMask[iPos];
					if (c != '#' && c != decimalPoint && c != groupSeparator)
					{
						m_behavior = new MaskedBehavior(this, value);
						return;
					}
				}

				// Verify that it ends in a number; otherwise it's a masked numeric
				if (smallLength > 0 && smallMask[smallLength - 1] != '#')
					m_behavior = new MaskedBehavior(this, value);
				else
					m_behavior = new NumericBehavior(this, value);
			}
		}
	}
}
