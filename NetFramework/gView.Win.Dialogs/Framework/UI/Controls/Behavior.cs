using System;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization; 
using System.ComponentModel;
using System.Runtime.InteropServices; 
using System.Collections;


namespace gView.Framework.UI.Controls
{
	/// <summary>
	///   Base class for all behavior classes in this namespace.
	///   It is designed to represent a behavior object that may be associated with a TextBoxBase object. </summary>
	/// <seealso cref="AlphanumericBehavior" />
	/// <seealso cref="MaskedBehavior" />
	/// <seealso cref="NumericBehavior" />
	/// <seealso cref="IntegerBehavior" />
	/// <seealso cref="CurrencyBehavior" />
	/// <seealso cref="DateBehavior" />
	/// <seealso cref="TimeBehavior" />
	/// <seealso cref="DateTimeBehavior" />
	public abstract class Behavior : IDisposable
	{		
		/// <summary> The TextBox object associated with this Behavior. </summary>
		protected TextBoxBase 	m_textBox;
		/// <summary> The flags turned on for this Behavior. </summary>
		protected int 			m_flags;		
		/// <summary> When true it indicates that HandleTextChanged should behave as if no text had changed and not call <see cref="UpdateText" />. </summary>
		protected bool 			m_noTextChanged;
		/// <summary> Helper object used to manipulate the selection of the TextBox object. </summary>
		protected Selection		m_selection;
		/// <summary> The object used to show a blinking icon (with an error) next to the control. </summary>		
		protected ErrorProvider m_errorProvider;
		/// <summary> The caption to use for all error message boxes. </summary>		
		private static string 	m_errorCaption;
		
		/// <summary>
		///   Initializes a new instance of the Behavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <param name="addEventHandlers">
		///   If true, the textBox's event handlers are tied to the corresponding methods on this behavior object. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor is <c>protected</c> since this class is only meant to be used as a base for other behaviors. </remarks>
		/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
		/// <seealso cref="AddEventHandlers" />	
		protected Behavior(TextBoxBase textBox, bool addEventHandlers)
		{
			if (textBox == null)
				throw new ArgumentNullException("textBox");
			
			m_textBox = textBox;
			m_selection = new Selection(m_textBox);
			m_selection.TextChanging += new EventHandler(HandleTextChangingBySelection);
			
			if (addEventHandlers)
				AddEventHandlers();				
		}

		/// <summary>
		///   Initializes a new instance of the Behavior class by copying it from 
		///   another Behavior object. </summary>
		/// <param name="behavior">
		///   The Behavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   This constructor is <c>protected</c> since this class is only meant to be used as a base for other behaviors. 
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		/// <seealso cref="TextBox" />	
		/// <seealso cref="Dispose" />	
		protected Behavior(Behavior behavior)
		{
			if (behavior == null)
				throw new ArgumentNullException("behavior");
			
			TextBox = behavior.TextBox;
			m_flags = behavior.m_flags;
			
			behavior.Dispose();
		}
		
		/// <summary>
		///   Handles the text changing as a result of direct manipulation of the selection. </summary>
		/// <remarks>
		///   This method sets m_noTextChanged flag to true so that UpdateText is not called 
		///   unnecessarily inside HandleTextChanged. </remarks>
		private void HandleTextChangingBySelection(object sender, EventArgs e)
		{
			m_noTextChanged = true;
		}

		/// <summary>
		///   Retrieves the textbox's text in valid form. </summary>
		/// <returns>
		///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
		/// <remarks>
		///   This method is designed to be overriden by derived Behavior classes.
		///   Here it just returns the textbox's text. </remarks>
		protected virtual string GetValidText()
		{
			return m_textBox.Text;
		}

		/// <summary>
		///   Checks if the textbox's text is valid and if not updates it with a valid value. </summary>
		/// <returns>
		///   If the textbox's text is updated (because it wasn't valid), the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   This method is used by derived classes to ensure the textbox's text is kept valid. </remarks>
		public virtual bool UpdateText()
		{			
			string validText = GetValidText();
			if (validText != m_textBox.Text)
			{
				m_textBox.Text = validText;
				return true;
			}
			return false;
		}

		/// <summary>
		///   Gets or sets the TextBoxBase object associated with this Behavior object (which is not allowed to be null). </summary>
		/// <exception cref="ArgumentNullException">TextBox is set to null. </exception>
		/// <remarks>
		///   Before the TextBoxBase object gets replaced, its event handlers are detached from this behavior object. 
		///   Then they're attached to the new object. </remarks>
		public TextBoxBase TextBox
		{
			get 
			{ 
				return m_textBox; 
			}
			set
			{			
				if (value == null)				
					throw new ArgumentNullException("value");

				RemoveEventHandlers();
			
				m_textBox = value;
				m_selection = new Selection(m_textBox);
				m_selection.TextChanging += new EventHandler(HandleTextChangingBySelection);
			
				AddEventHandlers();				
			}
		}

		/// <summary>
		///   Converts the given text to an integer. </summary>
		/// <returns>
		///   The return value is the text as an integer, or 0 if the conversion cannot be done. </returns>
		/// <remarks>
		///   This method serves as a convenience for derived Behavior classes that
		///   need to convert a string to an integer without worrying about a System.FormatException 
		///   exception being thrown. </remarks>
		/// <seealso cref="ToDouble" />	
		protected int ToInt(String text)
		{
			try
			{
				// Make it work like "atoi" -- ignore any trailing non-digit characters
				for (int i = 0, length = text.Length; i < length; i++)
				{
					if (!Char.IsDigit(text[i]))
						return Convert.ToInt32(text.Substring(0, i));
				}

				return Convert.ToInt32(text);
			}
			catch
			{
				return 0;
			}   				
		}		

		/// <summary>
		///   Converts the given text to a double. </summary>
		/// <returns>
		///   The return value is the text as a double, or 0 if the conversion cannot be done. </returns>
		/// <remarks>
		///   This method serves as a convenience for derived Behavior classes that
		///   need to convert a string to a double without worrying about a System.FormatException 
		///   exception being thrown. </remarks>
		/// <seealso cref="ToInt" />	
		protected double ToDouble(String text)
		{
			try
			{
				return Convert.ToDouble(text);
			}
			catch
			{
				return 0;
			}   				
		}		

		/// <summary>
		///   Gets or sets the flags associated with this Behavior object. </summary>
		/// <remarks>
		///   This property serves as a convenience for derived Behavior classes
		///   which can use it to store binary attributes (flags) inside its individual bits. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="ModifyFlags" />
		public virtual int Flags
		{
			get 
			{ 
				return m_flags; 
			}
			set
			{
				if (m_flags == value)
					return;
				
				m_flags = value;
				UpdateText();
			}
		}
		
		/// <summary>
		///   Adds or removes flags from the behavior. </summary>
		/// <param name="flags">
		///   The bits to be turned on (ORed) or turned off in the internal flags member. </param>
		/// <param name="addOrRemove">
		///   If true the flags are added, otherwise they're removed. </param>
		/// <remarks>
		///   This method is a convenient way to modify the <see cref="Flags" /> property without overwriting its value.
		///   If the internal flags value is changed, <see cref="UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="Flags" />
		public void ModifyFlags(int flags, bool addOrRemove)
		{
			if (addOrRemove)
				Flags = m_flags | flags;
			else
				Flags = m_flags & ~flags;
		}

		/// <summary>
		///   Checks if a flag value is set (turned on). </summary>
		/// <param name="flag">
		///   The flag to check. </param>
		/// <returns>
		///   If the flag is set, the return value is true; otherwise it is false. </returns>
		/// <seealso cref="Behavior.Flags" />
		public bool HasFlag(int flag)
		{
			return (m_flags & flag) != 0;
		}

		/// <summary>
		///   Shows an error message box. </summary>
		/// <param name="message">
		///   The message to show. </param>
		/// <remarks>
		///   Although doing so is not expected, this method may be overriden by derived classes. </remarks>
		public virtual void ShowErrorMessageBox(string message)
		{
			MessageBox.Show(m_textBox, message, ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		/// <summary>
		///   Shows a blinking icon next to the textbox with an error message. </summary>
		/// <param name="message">
		///   The message to show when the cursor is placed over the icon. </param>
		/// <remarks>
		///   Although doing so is not expected, this method may be overriden by derived classes. </remarks>
		public virtual void ShowErrorIcon(string message)
		{
			if (m_errorProvider == null)
			{
				if (message == "")
					return;
				m_errorProvider = new ErrorProvider();
			}
			m_errorProvider.SetError(m_textBox, message);
		}

		/// <summary>
		///   Gets the error message used to notify the user to enter a valid value. </summary>
		/// <remarks>
		///   This property is used by <see cref="Validate" /> to retrieve the message to 
		///   display in a message box or icon if the validation fails, depending on the flags set by the user.
		///   Here it just shows a generic error message, but it is meant to be overriden by 
		///   Behavior classes where either the allowed range of values is not controlled as 
		///   the user types (e.g. NumericBehavior, TimeBehavior), or the value is not considered 
		///   valid until the user has entered all the required characters (e.g. DateBehavior, TimeBehavior). </remarks>
		/// <seealso cref="Validate" />
		/// <seealso cref="IsValid" />
		/// <seealso cref="ErrorCaption" />
		public virtual string ErrorMessage
		{
			get
			{
				return "Please specify a valid value.";
			}
		}

		/// <summary>
		///   Gets or sets the caption to use for all error message boxes. </summary>
		/// <remarks>
		///   This property may be used to change the default caption (<see cref="Application.ProductName" />) used
		///   for all error message boxes shown via the <see cref="ShowErrorMessageBox" /> method. </remarks>
		/// <seealso cref="ShowErrorMessageBox" />
		public static string ErrorCaption
		{
			get 
			{ 
				if (m_errorCaption == null)
					return Application.ProductName;
				return m_errorCaption; 
			}
			set
			{
				m_errorCaption = value;				
			}
		}
		
		/// <summary>  
		///   If TRACE_AMS (and TRACE) are defined for the compiler, a message line is sent to the tracer. </summary>
		/// <param name="message">
		///   The message line to trace. </param>
		/// <remarks>
		///   This method is used to help diagnose problems.  It's called at the beginning of all 
		///   event handlers (the ones that begin with Handle) to trace the program's execution. </remarks>
		[Conditional("TRACE_AMS")]
		public void TraceLine(string message)
		{
			Trace.WriteLine(message);
		}
		                              
		/// <summary>
		///   Checks if the textbox's value is valid and if not proceeds according to the behavior's <see cref="Flags" />. </summary>
		/// <returns>
		///   If the validation succeeds, the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   This method is automatically called by the textbox's <see cref="Control.Validating" /> event if its 
		///   <see cref="Control.CausesValidation" /> property is set to true.
		///   It delegates to the overridable version of Validate. </remarks>
		/// <seealso cref="Control.Validating" />
		public bool Validate()
		{
			return Validate(Flags, false);
		}

		/// <summary>
		///   Checks if the textbox's value is valid and if not proceeds according to the given set of flags. </summary>
		/// <param name="flags">
		///   The combination of zero or more <see cref="ValidatingFlag" /> values added (ORed) together.
		///   This determines if the value should be checked for being empty, invalid, or neither, and then what action to take. </param>
		/// <param name="setFocusIfNotValid">
		///   If true and the validation fails (based on the flags parameter), the focus is placed on the textbox. </param>
		/// <returns>
		///   If the validation succeeds, the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   This method is indirectly called by the textbox's <see cref="Control.Validating" /> event if its 
		///   <see cref="Control.CausesValidation" /> property is set to true.
		///   Although doing so is not expected, this method may be overriden to provide extra validation in derived classes. </remarks>
		/// <seealso cref="IsValid" />
		/// <seealso cref="Control.Validating" />
		public virtual bool Validate(int flags, bool setFocusIfNotValid)
		{
			ShowErrorIcon("");  // clear the icon if it's being shown

			// Check if any of the flags are set
			if ((flags & (int)ValidatingFlag.Max) == 0)
				return true;

			// If we care about the value being empty, check and take the proper action
			if ((flags & (int)ValidatingFlag.Max_IfEmpty) != 0 && m_textBox.Text == "")
			{
				if ((flags & (int)ValidatingFlag.Beep_IfEmpty) != 0)
					MessageBeep(MessageBoxIcon.Exclamation);
					
				if ((flags & (int)ValidatingFlag.SetValid_IfEmpty) != 0)
				{
					UpdateText();
					return true;
				}

				if ((flags & (int)ValidatingFlag.ShowIcon_IfEmpty) != 0)
					ShowErrorIcon(ErrorMessage);		

				if ((flags & (int)ValidatingFlag.ShowMessage_IfEmpty) != 0)
					ShowErrorMessageBox(ErrorMessage);

				if (setFocusIfNotValid)
					m_textBox.Focus();

				return false;
			}
				
			// If we care about the value being invalid, check and take the proper action
			if ((flags & (int)ValidatingFlag.Max_IfInvalid) != 0 && m_textBox.Text != "" && !IsValid())
			{
				if ((flags & (int)ValidatingFlag.Beep_IfInvalid) != 0)
					MessageBeep(MessageBoxIcon.Exclamation);
					
				if ((flags & (int)ValidatingFlag.SetValid_IfInvalid) != 0)
				{
					UpdateText();
					return true;
				}
				
				if ((flags & (int)ValidatingFlag.ShowIcon_IfInvalid) != 0)
					ShowErrorIcon(ErrorMessage);		

				if ((flags & (int)ValidatingFlag.ShowMessage_IfInvalid) != 0)
					ShowErrorMessageBox(ErrorMessage);

				if (setFocusIfNotValid)
					m_textBox.Focus();

				return false;
			}

			return true;
		}

		/// <summary>
		///   Checks if the textbox contains a valid value. </summary>
		/// <returns>
		///   If the value is valid, the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   This method is called by the <see cref="Validate" /> to check validity. Here it just returns true, 
		///   but it is meant to be overriden by Behavior classes where either the allowed range of values is not
		///   controlled as the user types (e.g. NumericBehavior, TimeBehavior), or the value is not considered 
		///   valid until the user has entered all the required characters (e.g. DateBehavior, TimeBehavior). </remarks>
		public virtual bool IsValid()
		{
			return true;
		}

		/// <summary>
		///   Attaches several textBox event handlers to their corresponding virtual methods of the Behavior class. </summary>
		/// <remarks>
		///   To alter a textBox's behavior, these events may be needed: KeyDown, KeyPress, TextChanged, Validating, and LostFocus.
		///   This method binds those events to these virtual methods: HandleKeyDown, HandleKeyPress, HandleTextChanged, HandleValidating, and HandleLostFocus.
		///   Derived behavior classes may override any of these methods to accomodate their own requirements. </remarks>
		/// <seealso cref="HandleKeyDown" />
		/// <seealso cref="HandleKeyPress" />
		/// <seealso cref="HandleTextChanged" />
		/// <seealso cref="HandleLostFocus" />
		/// <seealso cref="RemoveEventHandlers" />
		protected virtual void AddEventHandlers()
		{
			m_textBox.KeyDown += new KeyEventHandler(HandleKeyDown);
			m_textBox.KeyPress += new KeyPressEventHandler(HandleKeyPress);
			m_textBox.TextChanged += new EventHandler(HandleTextChanged);
			m_textBox.Validating += new CancelEventHandler(HandleValidating);
			m_textBox.LostFocus += new EventHandler(HandleLostFocus);		
			m_textBox.DataBindings.CollectionChanged += new CollectionChangeEventHandler(HandleBindingChanges);
		}
			
		/// <summary>
		///   Dettaches several textBox event handlers from their corresponding virtual methods of the Behavior class. </summary>
		/// <remarks>
		///   This method does the opposite of <see cref="AddEventHandlers" /> and it allows a Behavior object to be associated with
		///   a textBox and later replaced by a different Behavior object. </remarks>
		/// <seealso cref="AddEventHandlers" />
		/// <seealso cref="Dispose" />
		protected virtual void RemoveEventHandlers()
		{
			if (m_textBox == null)
				return;
			
			m_textBox.KeyDown -= new KeyEventHandler(HandleKeyDown);
			m_textBox.KeyPress -= new KeyPressEventHandler(HandleKeyPress);
			m_textBox.TextChanged -= new EventHandler(HandleTextChanged);
			m_textBox.Validating -= new CancelEventHandler(HandleValidating);
			m_textBox.LostFocus -= new EventHandler(HandleLostFocus);		
			m_textBox.DataBindings.CollectionChanged -= new CollectionChangeEventHandler(HandleBindingChanges);
		}

		/// <summary>
		///   Disposes of the object by dettaching the textBox event handlers from their corresponding virtual 
		///   methods of the Behavior class and setting the Textbox to null. </summary>
		/// <seealso cref="RemoveEventHandlers" />
		public virtual void Dispose()
		{
			RemoveEventHandlers();
			m_textBox = null;
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is virtual so that it can be overriden by derived classes to accomodate their own behavior. 
		///   Here it just sets e.Handled to false so that the keydown can happen. </remarks>
		/// <seealso cref="AddEventHandlers" />
		/// <seealso cref="Control.KeyDown" />
		protected virtual void HandleKeyDown(object sender, KeyEventArgs e)
		{
			TraceLine("Behavior.HandleKeyDown " + e.KeyCode);
			
			e.Handled = false;
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is virtual so that it can be overriden by derived classes to accomodate their own behavior.
		///   Here it just sets e.Handled to false so that the keypress can happen. </remarks>
		/// <seealso cref="AddEventHandlers" />
		/// <seealso cref="Control.KeyPress" />
		protected virtual void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			TraceLine("Behavior.HandleKeyPress " + e.KeyChar);

			e.Handled = false;
		}
	
		/// <summary>
		///   Handles changes in the textbox text. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is virtual so that it can be overriden by derived classes to accomodate their own behavior.
		///   Here it calls <see cref="UpdateText" /> (unless the internal  <see cref="m_noTextChanged" /> flag is <c>true</c>) 
		///   to ensure the text is kept valid. </remarks>
		/// <seealso cref="AddEventHandlers" />
		/// <seealso cref="Control.TextChanged" />
		protected virtual void HandleTextChanged(object sender, EventArgs e)
		{
			TraceLine("Behavior.HandleTextChanged " + m_noTextChanged);
			
			if (!m_noTextChanged)
				UpdateText();
			
			m_noTextChanged = false;
		}

		/// <summary>
		///   Handles when the control is being validated as a result of losing its focus. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method calls <see cref="Validate" /> to determine if the textbox's value is valid and
		///   the return value is used to set <see cref="CancelEventArgs.Cancel">e.Cancel</see>.  
		///   Although not expected, this method may be overriden by derived classes to accomodate their own behavior. </remarks>
		/// <seealso cref="AddEventHandlers" />
		/// <seealso cref="Validate" />
		/// <seealso cref="Control.Validating" />
		protected virtual void HandleValidating(object sender, CancelEventArgs e)
		{
			TraceLine("Behavior.HandleValidating");				

			e.Cancel = !Validate();
		}
	
		/// <summary>
		///   Handles when the control has lost its focus. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is virtual so that it can be overriden by derived classes to accomodate their own behavior.
		///   Here it does nothing. </remarks>
		/// <seealso cref="AddEventHandlers" />
		/// <seealso cref="Control.LostFocus" />
		protected virtual void HandleLostFocus(object sender, EventArgs e)
		{			
			TraceLine("Behavior.HandleLostFocus");				
		}		

		/// <summary>
		///   Handles when changes are made to the DataBindings property of the control. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is virtual so that it can be overriden by derived classes to accomodate their own behavior.
		///   Here it checks if a Binding object has been added to the DataBindings collection
		///   so that its Parse event can be wired to the <see cref="HandleBindingFormat" /> and 
		///   <see cref="HandleBindingParse" />  methods.  </remarks>
		/// <seealso cref="HandleBindingFormat" />
		/// <seealso cref="HandleBindingParse" />
		/// <seealso cref="BindingsCollection.CollectionChanged" />
		protected virtual void HandleBindingChanges(object sender, CollectionChangeEventArgs e)
		{
			if (e.Action == CollectionChangeAction.Add)
			{
				Binding binding = (Binding)e.Element;
				binding.Format += new ConvertEventHandler(HandleBindingFormat);
				binding.Parse += new ConvertEventHandler(HandleBindingParse);
			}
		}

		/// <summary>
		///   Handles when the value of the object bound to this control needs to be formatted to be 
		///   placed on the control. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is virtual so that it can be overriden by derived classes to accomodate their own behavior.
		///   Here it does nothing. </remarks>
		/// <seealso cref="HandleBindingChanges" />
		/// <seealso cref="Binding.Format" />
		protected virtual void HandleBindingFormat(object sender, ConvertEventArgs e)
		{
		}			

		/// <summary>
		///   Handles when the control's text gets parsed to be converted to the type expected by the 
		///   object that it's bound to. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is virtual so that it can be overriden by derived classes to accomodate their own behavior.
		///   Here it checks if the control's text is empty so that it can set it to DBNull.Value. </remarks>
		/// <seealso cref="HandleBindingChanges" />
		/// <seealso cref="Binding.Parse" />
		protected virtual void HandleBindingParse(object sender, ConvertEventArgs e)
		{
			if (e.Value.ToString() == "")
				e.Value = DBNull.Value;
		}
			
		/// <summary>
		///   Makes a beeping sound. </summary>
		/// <param name="mbi">
		///   The type of sound to make, based on the situation. </param>
		[DllImport("user32.dll")]
		protected static extern bool MessageBeep(MessageBoxIcon mbi);				
	} 	


	/// <summary>
	///   Values that may be added/removed to the <see cref="Behavior.Flags" /> property related 
	///   to validating the textbox. </summary>
	/// <seealso cref="Behavior.ModifyFlags" />
	/// <seealso cref="Behavior.HasFlag" />
	/// <seealso cref="Behavior.Validate" />
	/// <seealso cref="Behavior.HandleValidating" />
	[Flags]
	public enum ValidatingFlag
	{
		/// <summary> If the value is not valid, make beeping sound. </summary>
		Beep_IfInvalid					= 0x00000001,

		/// <summary> If the value is empty, make beeping sound. </summary>
		Beep_IfEmpty					= 0x00000002,

		/// <summary> If the value is empty or not valid, make beeping sound. </summary>
		Beep							= Beep_IfInvalid | Beep_IfEmpty,

		/// <summary> If the value is not valid, change its value to something valid. </summary>
		SetValid_IfInvalid				= 0x00000004,

		/// <summary> If the value is empty, change its value to something valid. </summary>
		SetValid_IfEmpty				= 0x00000008,

		/// <summary> If the value is empty or not valid, change its value to something valid. </summary>
		SetValid						= SetValid_IfInvalid | SetValid_IfEmpty,

		/// <summary> If the value is not valid, show an error message box. </summary>
		ShowMessage_IfInvalid			= 0x00000010,

		/// <summary> If the value is empty, show an error message box. </summary>
		ShowMessage_IfEmpty				= 0x00000020,

		/// <summary> If the value is empty or not valid, show an error message box. </summary>
		ShowMessage						= ShowMessage_IfInvalid | ShowMessage_IfEmpty,

		/// <summary> If the value is not valid, show a blinking icon next to it. </summary>
		ShowIcon_IfInvalid				= 0x00000040,

		/// <summary> If the value is empty, show a blinking icon next to it. </summary>
		ShowIcon_IfEmpty				= 0x00000080,

		/// <summary> If the value is empty or not valid, show a blinking icon next to it. </summary>
		ShowIcon						= ShowIcon_IfInvalid | ShowIcon_IfEmpty,

		/// <summary> Combination of all IfInvalid flags (above); used internally by the program. </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		Max_IfInvalid					= Beep_IfInvalid | SetValid_IfInvalid | ShowMessage_IfInvalid | ShowIcon_IfInvalid,

		/// <summary> Combination of all IfEmpty flags (above); used internally by the program. </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		Max_IfEmpty						= Beep_IfEmpty | SetValid_IfEmpty | ShowMessage_IfEmpty | ShowIcon_IfEmpty,

		/// <summary> Combination of all flags; used internally by the program. </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		Max								= Max_IfInvalid + Max_IfEmpty
	};

		
	/////////////////////////////////////////////////////////////////////////////
	// Alphanumeric behavior

	/// <summary>
	///   Behavior class which prevents entry of one or more characters. </summary>
	/// <seealso cref="MaskedBehavior" />
	public class AlphanumericBehavior : Behavior
	{			
		// Fields
		private char[] m_invalidChars = {'%', '\'', '*', '"', '+', '?', '>', '<', ':', '\\'};
		
		/// <summary>
		///   Initializes a new instance of the AlphanumericBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor sets the invalid characters to <c>%</c>, <c>'</c>, <c>*</c>, <c>"</c>, <c>+</c>, <c>?</c>, <c>></c>, <c>&lt;</c>, <c>:</c>, and <c>\</c>. </remarks>
		/// <seealso cref="InvalidChars" />
		public AlphanumericBehavior(TextBoxBase textBox) :
			base(textBox, true)
		{					
		}
		
		/// <summary>
		///   Initializes a new instance of the AlphanumericBehavior class by associating it with a TextBoxBase derived object and setting its invalid characters. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <param name="invalidChars">
		///   An array of characters that should not be allowed. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		public AlphanumericBehavior(TextBoxBase textBox, char[] invalidChars) :
			base(textBox, true)
		{
			m_invalidChars = invalidChars;
		}
			
		/// <summary>
		///   Initializes a new instance of the AlphanumericBehavior class by associating it with a TextBoxBase derived object and setting its invalid characters. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <param name="invalidChars">
		///   The set of characters not allowed, concatenated into a string. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		public AlphanumericBehavior(TextBoxBase textBox, string invalidChars) :
			base(textBox, true)
		{
			m_invalidChars = invalidChars.ToCharArray();
		}

		/// <summary>
		///   Initializes a new instance of the AlphanumericBehavior class by copying it from 
		///   another AlphanumericBehavior object. </summary>
		/// <param name="behavior">
		///   The AlphanumericBehavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		public AlphanumericBehavior(AlphanumericBehavior behavior) :
			base(behavior)
		{
			m_invalidChars = behavior.m_invalidChars;			
		}

		/// <summary>
		///   Gets or sets the array of characters considered invalid (not allowed). </summary>
		/// <remarks>
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		public char[] InvalidChars
		{
			get 
			{ 
				return m_invalidChars; 
			}
			set 
			{ 
				if (m_invalidChars == value)
					return;
				
				m_invalidChars = value; 
				UpdateText(); 
			}
		}
		
		/// <summary>
		///   Retrieves the textbox's text in valid form. </summary>
		/// <returns>
		///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
		protected override string GetValidText()
		{
			string text = m_textBox.Text;
			
			// Check if there are any invalid characters and if so, remove them
			if (m_invalidChars != null && text.IndexOfAny(m_invalidChars) >= 0)
			{
				// There are invalid characters -- remove them
				foreach (char c in m_invalidChars)
				{
					if (text.IndexOf(c) >= 0)
						text = text.Replace(c.ToString(), "");
				}
			}
			
			// Check the max length
			if (text.Length > m_textBox.MaxLength)
				text = text.Remove(m_textBox.MaxLength, text.Length - m_textBox.MaxLength);
		
			return text;
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyPress event. </remarks>
		/// <seealso cref="Control.KeyPress" />
		protected override void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			TraceLine("AlphanumericBehavior.HandleKeyPress " + e.KeyChar);

			// Check to see if it's read only
			if (m_textBox.ReadOnly || m_invalidChars == null)
				return;
					
			char c = e.KeyChar;
			e.Handled = true;

			// Check if the character is invalid				
			if (Array.IndexOf(m_invalidChars, c) >= 0)
			{
				MessageBeep(MessageBoxIcon.Exclamation); 
				return;
			}
			
			// If the number of characters is already at Max, overwrite
			string text = m_textBox.Text;
			if (text.Length == m_textBox.MaxLength && m_textBox.MaxLength > 0 && !Char.IsControl(c))
			{
				int start, end;
				m_selection.Get(out start, out end);
		
				if (start < m_textBox.MaxLength)
					m_selection.SetAndReplace(start, start + 1, c.ToString());
					
				return;
			}
		
			base.HandleKeyPress(sender, e);					
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	// Masked behavior

	/// <summary>
	///   Behavior class which restricts input based on a mask containing one or more special symbols. </summary>
	/// <remarks>
	///   This class is useful for values with a rigid format, such as phone numbers, 
	///   social security numbers, or zip codes. </remarks>
	public class MaskedBehavior : Behavior
	{
		// Fields
		private string m_mask;
		private ArrayList m_symbols = new ArrayList();
	
		/// <summary>
		///   Initializes a new instance of the MaskedBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor sets the mask to an empty string, so that anything is allowed. </remarks>
		/// <seealso cref="Mask" />
		public MaskedBehavior(TextBoxBase textBox) :
			this(textBox, "")
		{
		}

		/// <summary>
		///   Initializes a new instance of the MaskedBehavior class by associating it with a TextBoxBase derived object and setting its mask. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <param name="mask">
		///   The mask string to use for validating and/or formatting the characters entered by the user. 
		///   By default, the <c>#</c> symbol is configured to represent a digit placeholder on the mask. </param>
		/// <example><c>MaskedBehavior behavior = new MaskedBehavior(txtPhoneNumber, "###-####"); </c></example>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <seealso cref="Mask" />
		/// <seealso cref="Symbols" />
		public MaskedBehavior(TextBoxBase textBox, string mask) :
			base(textBox, true)
		{
			m_mask = mask;
			
			// Add the default numeric symbol
			m_symbols.Add(new Symbol('#', new Symbol.ValidatorMethod(Char.IsDigit)));					
		}

		/// <summary>
		///   Initializes a new instance of the MaskedBehavior class by copying it from 
		///   another MaskedBehavior object. </summary>
		/// <param name="behavior">
		///   The MaskedBehavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		public MaskedBehavior(MaskedBehavior behavior) :
			base(behavior)
		{
			m_mask = behavior.m_mask;			
			m_symbols = behavior.m_symbols;
		}

		/// <summary>
		///   Gets or sets the mask. </summary>
		/// <remarks>
		///   This string is used for validating and/or formatting the characters entered by the user. 
		///   By default, the <c>#</c> symbol is configured to represent a digit placeholder on the mask. 
		///   Thus, each '#' symbol in the mask represents a digit, and any other characters between the 
		///   # symbols are automatically filled-in as the user types digits. 
		///   <para>
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </para></remarks>
		/// <seealso cref="Symbols" />
		public string Mask
		{
			get 
			{ 
				return m_mask; 
			}
			set 
			{ 
				if (m_mask == value)
					return;
				
				m_mask = value; 
				UpdateText(); 
			}
		}

		/// <summary>
		///   Gets the ArrayList of Symbol objects. </summary>
		/// <remarks>
		///   This array will initially contain one record: the one for the <c>#</c> symbol, which represents a digit placeholder on the mask. 
		///   However, more Symbol objects can be easily added to the array to make the mask more powerful. </remarks>
		/// <example><code>
		///   MaskedBehavior behavior = new MaskedBehavior(txtSerialNumber, "^#^-^##-###");
		///   
		///   // Add the ^ symbol to only allow letters and to convert them to upper-case. 
		///   MaskedBehavior.Symbol.ValidatorMethod validator = new MaskedBehavior.Symbol.ValidatorMethod(Char.IsLetter); 
		///   MaskedBehavior.Symbol.FormatterMethod formatter = new MaskedBehavior.Symbol.FormatterMethod(Char.ToUpper)));
		///   behavior.Symbols.Add(new MaskedBehavior.Symbol('^', validator, formatter)); </code></example>
		/// <seealso cref="Mask" />
		/// <seealso cref="Symbol" />
		public ArrayList Symbols
		{
			get 
			{ 
				return m_symbols; 
			}
		}

		/// <summary>
		///   Retrieves the textbox's value without any non-numeric characters. </summary>
		public string NumericText
		{
			get 
			{ 
				string text = m_textBox.Text;
				StringBuilder numericText = new StringBuilder();
			
				foreach (char c in text)
				{
					if (Char.IsDigit(c))
						numericText.Append(c);
				}
			
				return numericText.ToString();
			}					
		}

		/// <summary>
		///   Represents a character which may be added to the mask and then interpreted by the <see cref="MaskedBehavior" /> class 
		///   to validate the input from the user and possibly format it to something else. </summary>
		public class Symbol
		{
			/// <summary>
			///   Definition for the method used to check if the character entered by the user corresponds 
			///   with this object's symbol. </summary>
			/// <seealso cref="Validator" />
			/// <seealso cref="Validate" />
			/// <seealso cref="FormatterMethod" />
			public delegate bool ValidatorMethod(char c);

			/// <summary>
			///   Definition for the method used to format the character entered by the user to a different character, if needed. </summary>
			/// <seealso cref="Formatter" />
			/// <seealso cref="Format" />
			/// <seealso cref="ValidatorMethod" />
			public delegate char FormatterMethod(char c);

			/// <summary>
			///   Event used to check if the character entered by the user corresponds 
			///   with this object's symbol. </summary>
			/// <seealso cref="ValidatorMethod" />
			/// <seealso cref="Validate" />
			/// <seealso cref="Formatter" />
			public event ValidatorMethod Validator;

			/// <summary>
			///   Event used to format the character entered by the user to a different character, if needed. </summary>
			/// <seealso cref="FormatterMethod" />
			/// <seealso cref="Format" />
			/// <seealso cref="Validator" />
			public event FormatterMethod Formatter;
			
			// The symbol's character
			private char m_symbol;

			/// <summary>
			///   Initializes a new instance of the Symbol class by associating it with a character. </summary>
			/// <param name="symbol">
			///   The character that is represented by this object in the mask string. </param>
			/// <remarks>
			///   This constructor sets the validator and formatter methods to null. </remarks>
			/// <seealso cref="MaskedBehavior" />
			public Symbol(char symbol) :
				this(symbol, null, null)
			{
			}
			
			/// <summary>
			///   Initializes a new instance of the Symbol class by associating it with a character and 
			///   a validator method. </summary>
			/// <param name="symbol">
			///   The character that is represented by this object in the mask string. </param>
			/// <param name="validator">
			///   The method called to check if the character entered by the user corresponds 
			///   with this object's symbol. </param>
			/// <remarks>
			///   This constructor sets the formatter method to null, meaning that the character 
			///   entered by the user is not formatted. </remarks>
			/// <seealso cref="MaskedBehavior" />
			public Symbol(char symbol, ValidatorMethod validator) :
				this(symbol, validator, null)
			{
			}

			/// <summary>
			///   Initializes a new instance of the Symbol class by associating it with a character and 
			///   a validator method. </summary>
			/// <param name="symbol">
			///   The character that is represented by this object in the mask string. </param>
			/// <param name="validator">
			///   The method called to check if the character entered by the user corresponds 
			///   with this object's symbol. </param>
			/// <param name="formatter">
			///   The method called to format the character entered by the user to a different character, if needed. </param>
			/// <seealso cref="MaskedBehavior" />
			public Symbol(char symbol, ValidatorMethod validator, FormatterMethod formatter)
			{
				m_symbol = symbol;
				Validator = validator;
				Formatter = formatter;					
			}

			/// <summary>
			///   Checks if the character entered by the user corresponds with this object's symbol. </summary>
			/// <param name="c">
			///   The character entered by the user that needs to be checked. </param>
			/// <returns>
			///   If the character entered by the user is a valid representation of the symbol, 
			///   the return value is true; otherwise it is false. </returns>
			/// <remarks>
			///   This method may be overriden by derived classes to provide custom validation logic. </remarks> 
			/// <seealso cref="Format" />
			public virtual bool Validate(char c)
			{
				if (Validator != null)
				{
					foreach (ValidatorMethod validator in Validator.GetInvocationList())
					{
						if (!validator(c))
							return false;
					}
				}
				return true;
			}
			
			/// <summary>
			///   Formats the character entered by the user to a different character. </summary>
			/// <param name="c">
			///   The character entered by the user that will be formatted. </param>
			/// <returns>
			///   The reformatted character, as a string.  This allows derived classes more formatting flexibility if needed. </returns>
			/// <remarks>
			///   This method may be overriden by derived classes to provide custom formatting logic. 
			///   If no formatter method was associated with this object, the character is returned intact. </remarks> 
			/// <seealso cref="Validate" />
			public virtual string Format(char c)
			{
				if (Formatter != null)
					return Formatter(c).ToString();
				return c.ToString();
			}

			/// <summary>
			///   Gets or sets the character for this symbol. </summary>
			public char Char
			{
				get 
				{ 
					return m_symbol; 
				}
				set 
				{ 
					m_symbol = value; 
				}
			}

			/// <summary>
			///   Allows converting/casting a Symbol object to its character representation. </summary>
			/// <example><code>
			///   MaskedBehavior.Symbol s = new MaskedBehavior.Symbol('#');
			///   char c = s; </code></example>
			/// <seealso cref="Char" />
			public static implicit operator char(Symbol s)
			{
				return s.Char;
			}								
		}
		
		/// <summary>
		///   Retrieves the textbox's text in valid form. </summary>
		/// <returns>
		///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
		protected override string GetValidText()
		{
			string text = m_textBox.Text;
			int maskLength = m_mask.Length;
			
			// If the mask is empty, allow anything
			if (maskLength == 0)
				return text;
		
			StringBuilder validText = new StringBuilder();			
			int symbolCount = m_symbols.Count;
									
			// Accomodate the text to the mask as much as possible
			for (int iPos = 0, iMaskPos = 0, length = text.Length; iPos < length; iPos++, iMaskPos++)
			{
				char c = text[iPos];
				char cMask = (iMaskPos < maskLength ? m_mask[iMaskPos] : (char)0);
		
				// If we've reached the end of the mask, break
				if (cMask == 0)
					break;
		
				int iSymbol = 0;
				
				// Match the character to any of the symbols
				for (; iSymbol < symbolCount; iSymbol++)
				{
					Symbol symbol = (Symbol)m_symbols[iSymbol];
		
					// Find the symbol that applies for the given character
					if (!symbol.Validate(c))
						continue;
		
					// Try to add matching characters in the mask until a different symbol is reached
					for (; iMaskPos < maskLength; iMaskPos++)
					{
						cMask = m_mask[iMaskPos];
						if (cMask == (char)symbol)
						{
							validText.Append(symbol.Format(c));
							break;
						} 
						else
						{
							int iSymbol2 = 0;
							for (; iSymbol2 < symbolCount; iSymbol2++)
							{
								Symbol symbol2 = (Symbol)m_symbols[iSymbol2];
								if (cMask == (char)symbol2)
								{
									validText.Append(symbol.Format(c));
									break;
								}
							}
		
							if (iSymbol2 < symbolCount)
								break;
		
							validText.Append(cMask);
						}
					}
		
					break;
				}
		
				// If the character was not matched to a symbol, stop
				if (iSymbol == symbolCount)
				{
					if (c == cMask)
					{
						// Match the character to any of the symbols
						for (iSymbol = 0; iSymbol < symbolCount; iSymbol++)
						{
							Symbol symbol = (Symbol)m_symbols[iSymbol];
							if (cMask == (char)symbol)
								break;
						}
		
						if (iSymbol == symbolCount)
						{
							validText.Append(c);
							continue;
						}
					}
		
					break;
				}
			}
		
			return validText.ToString();						
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyDown event. </remarks>
		/// <seealso cref="Control.KeyDown" />
		protected override void HandleKeyDown(object sender, KeyEventArgs e)
		{
			TraceLine("MaskedBehavior.HandleKeyDown " + e.KeyCode);

			if (e.KeyCode == Keys.Delete)
			{
				// If deleting make sure it's the last character or that
				// the selection goes all the way to the end of the text
	
				int start, end;
				m_selection.Get(out start, out end);
	
				string text = m_textBox.Text;
				int length = text.Length;
	
				if (end != length)
				{
					if (!(end == start && end == length - 1))
						e.Handled = true;
				}
			}
		}								

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyPress event. </remarks>
		/// <seealso cref="Control.KeyPress" />
		protected override void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			TraceLine("MaskedBehavior.HandleKeyPress " + e.KeyChar);

			// Check to see if it's read only
			if (m_textBox.ReadOnly)
				return;
					
			char c = e.KeyChar;
			e.Handled = true;
		
			// If the mask is empty, allow anything
			int maskLength = m_mask.Length;
			if (maskLength == 0)
			{
				base.HandleKeyPress(sender, e);
				return;
			}
		
			int start, end;
			m_selection.Get(out start, out end);

			// Check that we haven't gone past the mask's length
			if (start >= maskLength && c != (short)Keys.Back)
				return;
					
			string text = m_textBox.Text;
			int length = text.Length;
		
			// Check for a non-printable character (such as Ctrl+C)
			if (Char.IsControl(c))
			{
				if (c == (short)Keys.Back && start != length)
				{
					SendKeys.Send("{LEFT}");  // move the cursor left
					return;
				}
				
				// Allow backspace only if the cursor is all the way to the right
				base.HandleKeyPress(sender, e);
				return;
			}
		
			char cMask = m_mask[start];
			
			// Check if the mask's character matches with any of the symbols in the array.
			foreach (Symbol symbol in m_symbols)
			{						
				if (cMask == (char)symbol)
				{
					if (symbol.Validate(c))
					{
						end = (end == length ? end : (start + 1));
						m_selection.SetAndReplace(start, end, symbol.Format(c));
					}
					return;
				}
			}
		
			// Check if it's the same character as the mask.
			if (cMask == c)
			{
				end = (end == length ? end : (start + 1));
				m_selection.SetAndReplace(start, end, c.ToString());
				return;
			}
		
			// Concatenate all the mask symbols
			StringBuilder concatenatedSymbols = new StringBuilder();
			foreach (Symbol symbol in m_symbols)
				concatenatedSymbols.Append((char)symbol);
			
			char[] symbolChars = concatenatedSymbols.ToString().ToCharArray();
		
			// If it's a valid character, find the next symbol on the mask and add any non-mask characters in between.
			foreach (Symbol symbol in m_symbols)
			{
				// See if the character is valid for any other symbols
				if (!symbol.Validate(c))
					continue;
		
				string maskPortion = m_mask.Substring(start);
				int maskPos = maskPortion.IndexOfAny(symbolChars);
					
				// Enter the character if there isn't another symbol before it
				if (maskPos >= 0 && maskPortion[maskPos] == (char)symbol)
				{
					m_selection.SetAndReplace(start, start + maskPos, maskPortion.Substring(0, maskPos));
					HandleKeyPress(sender, e);
					return;
				}
			}
		}			
	}


	/////////////////////////////////////////////////////////////////////////////
	// Numeric behavior
	
	/// <summary>
	///   Behavior class which handles numeric input. </summary>
	/// <remarks>
	///   This is the base class for the other numeric behavior classes.  
	///   It ensures that the user enters a valid number and provides features such as automatic formatting. 
	///   It also allows precise control over what the number can look like, such as how many digits to the 
	///   left and right of the decimal point, and whether it can be negative or not. </remarks>
	public class NumericBehavior : Behavior
	{
		// Fields
		private int		m_maxWholeDigits = 9;
		private int		m_maxDecimalPlaces = 4;				
		private int		m_digitsInGroup = 0;
		private char	m_negativeSign = NumberFormatInfo.CurrentInfo.NegativeSign[0];
		private char	m_decimalPoint = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];
		private char	m_groupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator[0];
		private string	m_prefix = "";
		private double	m_rangeMin = Double.MinValue;
		private double	m_rangeMax = Double.MaxValue;		

		private int		m_previousSeparatorCount = -1;
		private bool  	m_textChangedByKeystroke = false;

		/// <summary>
		///   Internal values that are added/removed to the <see cref="Behavior.Flags" /> property by other
		///   properties of this class. </summary>
		[Flags]		
		protected enum Flag
		{				
			/// <summary> The value is not allowed to be negative; the user is not allowed to enter a negative sign. </summary>
			CannotBeNegative							= 0x00010000, 

			/// <summary> If the user enters a digit after the <see cref="MaxWholeDigits" /> have been entered, a <see cref="DecimalPoint" /> is inserted and then the digit. </summary>
			AddDecimalAfterMaxWholeDigits				= 0x00020000			
		};			
		
		/// <summary>
		///   Values that may be added/removed to the <see cref="Behavior.Flags" /> property related to 
		///   what occurs when the textbox loses focus. </summary>
		/// <seealso cref="Behavior.ModifyFlags" />
		/// <seealso cref="Behavior.HasFlag" />
		[Flags]		
		public enum LostFocusFlag
		{				
			/// <summary> When the textbox loses focus, pad the value with up to <see cref="MaxWholeDigits" /> zeros left of the decimal symbol. </summary>
			PadWithZerosBeforeDecimal					= 0x00000100,

			/// <summary> When the textbox loses focus, pad the value with up to <see cref="MaxDecimalPlaces" /> zeros right of the decimal symbol. </summary>
			PadWithZerosAfterDecimal					= 0x00000200,

			/// <summary> When combined with the <see cref="PadWithZerosBeforeDecimal" /> or <see cref="PadWithZerosAfterDecimal" />, the padding is only done if the textbox is not empty. </summary>
			DontPadWithZerosIfEmpty						= 0x00000400,

			/// <summary> Insignificant zeros are removed from the numeric value left of the decimal point, unless the number itself is 0. </summary>
			RemoveExtraLeadingZeros						= 0x00000800,
			
			/// <summary> Combination of all the above flags; used internally by the program. </summary>
			[EditorBrowsable(EditorBrowsableState.Never)] Max = 0x00000F00,
			
			/// <summary> If the Text property is set, the LostFocus handler is called. </summary>
			CallHandlerWhenTextPropertyIsSet 			= 0x00001000,

			/// <summary> If the text changes, the LostFocus handler is called. </summary>
			CallHandlerWhenTextChanges					= 0x00002000			
		};	

		/// <summary>
		///   Initializes a new instance of the NumericBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor sets <see cref="MaxWholeDigits" /> = 9, <see cref="MaxDecimalPlaces" /> = 4, 
		///   <see cref="DigitsInGroup" /> = 0, <see cref="Prefix" /> = "", <see cref="AllowNegative" /> = true, 
		///   and the rest of the properties according to user's system. </remarks>
		public NumericBehavior(TextBoxBase textBox) :
			base(textBox, true)
		{
			AdjustDecimalAndGroupSeparators();					
		}
	
		/// <summary>
		///   Initializes a new instance of the NumericBehavior class by associating it with a TextBoxBase derived object
		///   and setting the maximum number of digits allowed left and right of the decimal point. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <param name="maxWholeDigits">
		///   The maximum number of digits allowed left of the decimal point.
		///   If it is less than 1, it is set to 1. </param>
		/// <param name="maxDecimalPlaces">
		///   The maximum number of digits allowed right of the decimal point.
		///   If it is less than 0, it is set to 0. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor sets <see cref="DigitsInGroup" /> = 0, <see cref="Prefix" /> = "", <see cref="AllowNegative" /> = true, 
		///   and the rest of the properties according to user's system. </remarks>
		/// <seealso cref="MaxWholeDigits" />
		/// <seealso cref="MaxDecimalPlaces" />
		public NumericBehavior(TextBoxBase textBox, int maxWholeDigits, int maxDecimalPlaces) :
			this(textBox)
		{
			m_maxWholeDigits = maxWholeDigits;
			m_maxDecimalPlaces = maxDecimalPlaces;					

			if (m_maxWholeDigits < 1)
				m_maxWholeDigits = 1;
			if (m_maxDecimalPlaces < 0)
				m_maxDecimalPlaces = 0;					
		}

		/// <summary>
		///   Initializes a new instance of the NumericBehavior class by associating it with a TextBoxBase derived object
		///   and assiging its attributes from a mask string. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <param name="mask">
		///   The string used to set several of the object's properties. 
		///   See <see cref="Mask" /> for more information. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor sets <see cref="AllowNegative" /> = true
		///   and the rest of the properties using the mask. </remarks>
		/// <seealso cref="Mask" />
		public NumericBehavior(TextBoxBase textBox, string mask) :
			base(textBox, true)
		{
			Mask = mask;
		}

		/// <summary>
		///   Initializes a new instance of the NumericBehavior class by copying it from 
		///   another NumericBehavior object. </summary>
		/// <param name="behavior">
		///   The NumericBehavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		public NumericBehavior(NumericBehavior behavior) :
			base(behavior)
		{
			m_maxWholeDigits = behavior.m_maxWholeDigits;
			m_maxDecimalPlaces = behavior.m_maxDecimalPlaces;				
			m_digitsInGroup = behavior.m_digitsInGroup;
			m_negativeSign = behavior.m_negativeSign;
			m_decimalPoint = behavior.m_decimalPoint;
			m_groupSeparator = behavior.m_groupSeparator;
			m_prefix = behavior.m_prefix;
			m_rangeMin = behavior.m_rangeMin;
			m_rangeMax = behavior.m_rangeMax;
		}

		/// <summary>
		///   Gets or sets the maximum number of digits allowed left of the decimal point. </summary>
		/// <remarks>
		///   If this property is set to a number less than 1, it is set to 1. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="MaxDecimalPlaces" />
		public int MaxWholeDigits
		{
			get 
			{ 
				return m_maxWholeDigits; 
			}
			set 
			{ 
				if (m_maxWholeDigits == value)
					return;
				
				m_maxWholeDigits = value;
				if (m_maxWholeDigits < 1)
					m_maxWholeDigits = 1;
				
				UpdateText(); 
			}
		}

		/// <summary>
		///   Gets or sets the maximum number of digits allowed right of the decimal point. </summary>
		/// <remarks>
		///   If this property is set to a number less than 0, it is set to 0. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="MaxWholeDigits" />
		public int MaxDecimalPlaces
		{
			get 
			{ 
				return m_maxDecimalPlaces; 
			}
			set 
			{ 
				if (m_maxDecimalPlaces == value)
					return;
				
				m_maxDecimalPlaces = value; 
				if (m_maxDecimalPlaces < 0)
					m_maxDecimalPlaces = 0;
				
				UpdateText(); 
			}
		}

		/// <summary>
		///   Gets or sets whether the value is allowed to be negative or not. </summary>
		/// <remarks>
		///   By default, this property is set to true, meaning that negative values are allowed. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="NegativeSign" />
		public bool AllowNegative
		{
			get 
			{ 
				return !HasFlag((int)Flag.CannotBeNegative); 
			}
			set 
			{ 
				ModifyFlags((int)Flag.CannotBeNegative, !value);	
			}
		}

		/// <summary>
		///   Gets or sets whether a <see cref="DecimalPoint" /> is automatically inserted if the user enters a digit 
		///   after the <see cref="MaxWholeDigits" /> have been entered </summary>
		/// <remarks>
		///   By default, this property is set to false, meaning that when the <see cref="MaxWholeDigits" /> have
		///   been entered, a <see cref="DecimalPoint" /> is not automaticall inserted -- the user has to do it manually. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="NegativeSign" />
		public bool AddDecimalAfterMaxWholeDigits
		{
			get 
			{ 
				return HasFlag((int)Flag.AddDecimalAfterMaxWholeDigits); 
			}
			set 
			{ 
				ModifyFlags((int)Flag.AddDecimalAfterMaxWholeDigits, value);	
			}
		}

		/// <summary>
		///   Gets or sets the number of digits to place in each group to the left of the decimal point. </summary>
		/// <remarks>
		///   By default, this property is set to 0. It may be set to 3 to group thousands using the <see cref="GroupSeparator">group separator</see>. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="GroupSeparator" />
		public int DigitsInGroup
		{
			get 
			{ 
				return m_digitsInGroup; 
			}
			set 
			{ 
				if (m_digitsInGroup == value)
					return;
				
				m_digitsInGroup = value; 
				if (m_digitsInGroup < 0)
					m_digitsInGroup = 0;
				
				UpdateText(); 
			}
		}

		/// <summary>
		///   Gets or sets the character to use for the decimal point. </summary>
		/// <remarks>
		///   By default, this property is set based on the user's system settings. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="GroupSeparator" />
		public char DecimalPoint
		{
			get 
			{ 
				return m_decimalPoint; 
			}
			set 
			{ 
				if (m_decimalPoint == value)
					return;
				
				m_decimalPoint = value; 
				AdjustDecimalAndGroupSeparators();
				UpdateText(); 
			}
		}

		/// <summary>
		///   Gets or sets the character to use for the group separator. </summary>
		/// <remarks>
		///   By default, this property is set based on the user's system settings. 
		///   In the US, this is typically a comma and it is used to separate the thousands. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="GroupSeparator" />
		public char GroupSeparator
		{
			get 
			{ 
				return m_groupSeparator; 
			}
			set 
			{ 
				if (m_groupSeparator == value)
					return;
				
				m_groupSeparator = value; 
				AdjustDecimalAndGroupSeparators();
				UpdateText(); 
			}
		}

		/// <summary>
		///   Gets or sets the character to use for the negative sign. </summary>
		/// <remarks>
		///   By default, this property is set based on the user's system settings, but it will likely be a minus sign. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="AllowNegative" />
		public char NegativeSign
		{
			get 
			{ 
				return m_negativeSign; 
			}
			set 
			{ 
				if (m_negativeSign == value)
					return;
				
				m_negativeSign = value; 
				UpdateText(); 
			}
		}

		/// <summary>
		///   Gets or sets the text to automatically insert in front of the number, such as a currency symbol. </summary>
		/// <remarks>
		///   By default, this property is set to an empty string. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		public String Prefix
		{
			get 
			{ 
				return m_prefix; 
			}
			set 
			{ 
				if (m_prefix == value)
					return;
				
				m_prefix = value; 
				UpdateText(); 
			}
		}

		/// <summary>
		///   Gets or sets the minimum value allowed. </summary>
		/// <remarks>
		///   By default, this property is set to <see cref="Double.MinValue" />, however the range is 
		///   only checked when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
		/// <seealso cref="RangeMax" />
		public double RangeMin
		{
			get 
			{ 
				return m_rangeMin; 
			}
			set 
			{ 
				m_rangeMin = value; 
			}
		}

		/// <summary>
		///   Gets or sets the maximum value allowed. </summary>
		/// <remarks>
		///   By default, this property is set to <see cref="Double.MaxValue" />, however the range is 
		///   only checked when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
		/// <seealso cref="RangeMin" />
		public double RangeMax
		{
			get 
			{ 
				return m_rangeMax; 
			}
			set 
			{ 
				m_rangeMax = value; 
			}
		}

		/// <summary>
		///   If necessary, adjusts the decimal and group separators so they're not the same. </summary>
		/// <remarks>
		///   If the decimal and group separators are found to be same, they are adjusted to be different. 
		///   This prevents potential problems as the user is entering the value. </remarks>	
		protected void AdjustDecimalAndGroupSeparators()
		{
			if (m_decimalPoint == m_groupSeparator)
				m_groupSeparator = (m_decimalPoint == ',' ? '.' : ',');						
		}
		
		/// <summary>
		///   Gets or sets a mask value representative of this object's properties. </summary>
		/// <remarks>
		///   This property can be set to configure these other properties: <see cref="MaxWholeDigits" />,
		///   <see cref="MaxDecimalPlaces" />, <see cref="DigitsInGroup" />, <see cref="Prefix" />, <see cref="DecimalPoint" />, 
		///   and <see cref="GroupSeparator" />.
		///   <para>
		///   For example, <c>"$ #,###.##"</c> means MaxWholeDigits = 4, MaxDecimalPlaces = 2, DigitsInGroup = 3, 
		///   Prefix = "$ ", DecimalPoint = '.', and GroupSeparator = ','. </para>
		///   <para>
		///   The # character is used to denote a digit placeholder. </para> </remarks>
		public string Mask
		{
			get
			{
				StringBuilder mask = new StringBuilder();
			
				for (int iDigit = 0; iDigit < m_maxWholeDigits; iDigit++)
					mask.Append('0');
			
				if (m_maxDecimalPlaces > 0)
					mask.Append(m_decimalPoint);
			
				for (int iDigit = 0; iDigit < m_maxDecimalPlaces; iDigit++)
					mask.Append('0');
			
				mask = new StringBuilder(GetSeparatedText(mask.ToString()));
			
				for (int iPos = 0, length = mask.Length; iPos < length; iPos++)
				{
					if (mask[iPos] == '0')
						mask[iPos] = '#';
				}
			
				return mask.ToString();
			}	
			set
			{
				int decimalPos = -1;
				int length = value.Length;
			
				m_maxWholeDigits = 0;
				m_maxDecimalPlaces = 0;
				m_digitsInGroup = 0;
				m_flags = (m_flags & (int)~Flag.CannotBeNegative);  // allow it to be negative
				m_prefix = "";
			
				for (int iPos = length - 1; iPos >= 0; iPos--)
				{
					char c = value[iPos];
					if (c == '#')
					{
						if (decimalPos >= 0)
							m_maxWholeDigits++;
						else
							m_maxDecimalPlaces++;
					}
					else if ((c == '.' || c == m_decimalPoint) && decimalPos < 0)
					{
						decimalPos = iPos;
						m_decimalPoint = c;
					}
					else if (c == ',' || c == m_groupSeparator)
					{
						if (m_digitsInGroup == 0)
						{
							m_digitsInGroup = (((decimalPos >= 0) ? decimalPos : length) - iPos) - 1;
							m_groupSeparator = c;
						}
					}
					else
					{
						m_prefix = value.Substring(0, iPos + 1);
						break;
					}
				}
			
				if (decimalPos < 0)
				{
					m_maxWholeDigits = m_maxDecimalPlaces;
					m_maxDecimalPlaces = 0;
				}
			
				Debug.Assert(m_maxWholeDigits > 0);	// must have at least one digit on left side of decimal point
				
				AdjustDecimalAndGroupSeparators();
				UpdateText();
			}		
		}
		
		/// <summary>
		///   Copies a string while inserting zeros into it. </summary>
		/// <param name="text">
		///   The text to copy with the zeros inserted. </param>
		/// <param name="startIndex">
		///   The zero-based position where the zeros should be inserted. 
		///   If this is less than 0, the zeros are appended. </param>
		/// <param name="count">
		///   The number of zeros to insert. </param>
		/// <returns>
		///   The return value is a copy of the text with the zeros inserted. </returns>
		protected string InsertZeros(string text, int startIndex, int count)
		{
			if (startIndex < 0 && count > 0)
				startIndex = text.Length;
			
			StringBuilder result = new StringBuilder(text);
			for (int iZero = 0; iZero < count; iZero++)
				result.Insert(startIndex, '0');
			
			return result.ToString();
		}
		
		/// <summary>
		///   Checks if the textbox's numeric value is within the allowed range. </summary>
		/// <returns>
		///   If the value is within the allowed range, the return value is true; otherwise it is false. </returns>
		/// <seealso cref="RangeMin" />
		/// <seealso cref="RangeMax" />
		public override bool IsValid()
		{
			double value = ToDouble(RealNumericText);
			return (value >= m_rangeMin && value <= m_rangeMax);
		}
		
		/// <summary>
		///   Gets the error message used to notify the user to enter a valid numeric value 
		///   within the allowed range. </summary>
		/// <seealso cref="IsValid" />
		public override string ErrorMessage
		{
			get
			{
				if (m_rangeMin > double.MinValue && m_rangeMax < double.MaxValue)
					return "Please specify a numeric value between " + m_rangeMin.ToString() + " and " + m_rangeMax.ToString() + ".";
				else if (m_rangeMin > double.MinValue)
					return "Please specify a numeric value greater than or equal to " + m_rangeMin.ToString() + ".";
				else if (m_rangeMax < double.MinValue)
					return "Please specify a numeric value less than or equal to " + m_rangeMax.ToString() + ".";
				return "Please specify a valid numeric value.";
			}
		}
		
		/// <summary>
		///   Adjusts the textbox's value to be within the range of allowed values. </summary>
		protected void AdjustWithinRange()
		{
			// Check if it's already within the range
			if (IsValid())
				return;
		
			// If it's empty, set it a valid number
			if (m_textBox.Text == "")
				m_textBox.Text = " ";
			else
				UpdateText();
		
			// Make it fall within the range
			double value = ToDouble(RealNumericText);
			if (value < m_rangeMin)
				m_textBox.Text = m_rangeMin.ToString(); 
			else if (value > m_rangeMax)
				m_textBox.Text = m_rangeMax.ToString(); 
		}	

		/// <summary>
		///   Retrieves the textbox's value without any non-numeric characters. </summary>
		/// <seealso cref="RealNumericText" />
		public string NumericText
		{
			get 
			{ 
				return GetNumericText(m_textBox.Text, false);	
			}					
		}

		/// <summary>
		///   Retrieves the textbox's value without any non-numeric characters,
		///   and with a period for the decimal point and a minus for the negative sign. </summary>
		/// <seealso cref="NumericText" />
		public string RealNumericText
		{
			get 
			{ 
				return GetNumericText(m_textBox.Text, true);	
			}					
		}

		/// <summary>
		///   Copies a string while removing any non-numeric characters from it. </summary>
		/// <param name="text">
		///   The text to parse and copy. </param>
		/// <param name="realNumeric">
		///   If true, the value is returned as a real number 
		///   (with a period for the decimal point and a minus for the negative sign);
		///   otherwise, it is returned using the expected symbols. </param>
		/// <returns>
		///   The return value is a copy of the original text containing only numeric characters. </returns>
		protected string GetNumericText(string text, bool realNumeric)
		{
			StringBuilder numericText = new StringBuilder();
			bool isNegative = false;
			bool hasDecimalPoint = false;

			foreach (char c in text)
			{
				if (Char.IsDigit(c))
					numericText.Append(c);
				else if (c == m_negativeSign)
					isNegative = true;
				else if (c == m_decimalPoint && !hasDecimalPoint)
				{
					hasDecimalPoint = true;
					numericText.Append(realNumeric ? '.' : m_decimalPoint);
				}
			}
		
			// Add the negative sign to the front of the number.
			if (isNegative)
				numericText.Insert(0, realNumeric ? '-' : m_negativeSign);

			return numericText.ToString();
		}

		/// <summary>
		///   Returns the number of group separator characters in the given text. </summary>
		private int GetGroupSeparatorCount(string text)
		{ 
			int count = 0;			
			foreach (char c in text)
			{
				if (c == m_groupSeparator)
					count++;
			}		
			return count;
		}

		/// <summary>
		///   Retrieves the textbox's text in valid form. </summary>
		/// <returns>
		///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
		protected override string GetValidText()
		{
			string text = m_textBox.Text;
			StringBuilder newText = new StringBuilder();			
			bool isNegative = false;
			int prefixLength = m_prefix.Length;
		
			// Remove any invalid characters from the number
			for (int iPos = 0, decimalPos = -1, newLength = 0, length = text.Length; iPos < length; iPos++)
			{
				char c = text[iPos];
		
				// Check for a negative sign
				if (c == m_negativeSign && AllowNegative)
					isNegative = true;
				
				// Check for a digit
				else if (Char.IsDigit(c))
				{
					// Make sure it doesn't go beyond the limits
					if (decimalPos < 0 && newLength == m_maxWholeDigits)
						continue;
		
					if (decimalPos >= 0 && newLength > decimalPos + m_maxDecimalPlaces)
						break;
		
					newText.Append(c);
					newLength++;
				}
				
				// Check for a decimal point
				else if (c == m_decimalPoint && decimalPos < 0)
				{
					if (m_maxDecimalPlaces == 0)
						break;
		
					newText.Append(c);
					decimalPos = newLength;
					newLength++;
				}
			}
			
			// Insert the negative sign if it's there
			if (isNegative)
				newText.Insert(0, m_negativeSign);
		
			return GetSeparatedText(newText.ToString());
		}

		/// <summary>
		///   Takes a piece of text containing a numeric value and inserts
		///   group separators in the proper places. </summary>
		/// <param name="text">
		///   The text to parse. </param>
		/// <returns>
		///   The return value is a copy of the original text with the group separators inserted. </returns>
		protected string GetSeparatedText(string text)
		{
			string numericText = GetNumericText(text, false);
			string separatedText = numericText;
			
			// Retrieve the number without the decimal point
			int decimalPos = numericText.IndexOf(m_decimalPoint);
			if (decimalPos >= 0)
				separatedText = separatedText.Substring(0, decimalPos);
		
			if (m_digitsInGroup > 0)
			{
				int length = separatedText.Length;
				bool isNegative = (separatedText != "" && separatedText[0] == m_negativeSign);
		
				// Loop in reverse and stick the separator every m_digitsInGroup digits.
				for (int iPos = length - (m_digitsInGroup + 1); iPos >= (isNegative ? 1 : 0); iPos -= m_digitsInGroup)
					separatedText = separatedText.Substring(0, iPos + 1) + m_groupSeparator + separatedText.Substring(iPos + 1);
			}
		
			// Prepend the prefix, if the number is not empty.
			if (separatedText != "" || decimalPos >= 0)
			{
				separatedText = m_prefix + separatedText;
		
				if (decimalPos >= 0)
					separatedText += numericText.Substring(decimalPos);
			}
		
			return separatedText;
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyDown event. </remarks>
		/// <seealso cref="Control.KeyDown" />
		protected override void HandleKeyDown(object sender, KeyEventArgs e)
		{
			TraceLine("NumericBehavior.HandleKeyDown " + e.KeyCode);

			if (e.KeyCode == Keys.Delete)
			{
				int start, end;
				m_selection.Get(out start, out end);
	
				string text = m_textBox.Text;
				int length = text.Length;
	
				// If deleting the prefix, don't allow it if there's a number after it.
				int prefixLength = m_prefix.Length;
				if (start < prefixLength && length > prefixLength)
				{
					if (end != length)
						e.Handled = true;
					return;
				}
	
				m_textChangedByKeystroke = true;
				
				// If deleting a group separator (comma), move the cursor to the right
				if (start < length && text[start] == m_groupSeparator && start == end)
					SendKeys.SendWait("{RIGHT}");
				
				m_previousSeparatorCount = GetGroupSeparatorCount(text);
				
				// If everything on the right was deleted, put the selection on the right
				if (end == length)
					SendKeys.Send("{RIGHT}");
			}				
		}
		
		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyPress event. </remarks>
		/// <seealso cref="Control.KeyPress" />
		protected override void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			TraceLine("NumericBehavior.HandleKeyPress " + e.KeyChar);

			// Check to see if it's read only
			if (m_textBox.ReadOnly)
				return;
					
			char c = e.KeyChar;
			e.Handled = true;
			m_textChangedByKeystroke = true;
		
			int start, end;
			m_selection.Get(out start, out end);

			string text = m_textBox.Text;
			m_previousSeparatorCount = -1;
			
			string numericText = NumericText;
			int decimalPos = text.IndexOf(m_decimalPoint);
			int numericDecimalPos = numericText.IndexOf(m_decimalPoint);
			int length = text.Length;
			int numericLen = numericText.Length;
			int prefixLength = m_prefix.Length;
			int separatorCount = GetGroupSeparatorCount(text);
		
			// Check if we're in the prefix's location
			if (start < prefixLength && !Char.IsControl(c))
			{
				char cPrefix = m_prefix[start];
		
				// Check if it's the same character as the prefix.
				if (cPrefix == c)
				{
					if (length > start)
					{
						end = (end == length ? end : (start + 1));
						m_selection.SetAndReplace(start, end, c.ToString());
					}
					else
						base.HandleKeyPress(sender, e);
				}
				// If it's a part of the number, enter the prefix
				else if (Char.IsDigit(c) || c == m_negativeSign || c == m_decimalPoint)
				{
					end = (end == length ? end : prefixLength);
					m_selection.SetAndReplace(start, end, m_prefix.Substring(start));		
					HandleKeyPress(sender, e);
				}
				
				return;
			}
		
			// Check if it's a negative sign
			if (c == m_negativeSign && AllowNegative)
			{
				// If it's at the beginning, determine if it should overwritten
				if (start == prefixLength)
				{
					if (numericText != "" && numericText[0] == m_negativeSign)
					{
						end = (end == length ? end : (start + 1));
						m_selection.SetAndReplace(start, end, m_negativeSign.ToString());		
						return;
					}
				}
				// If we're not at the beginning, toggle the sign
				else
				{
					if (numericText[0] == m_negativeSign)
					{
						m_selection.SetAndReplace(prefixLength, prefixLength + 1, "");
						m_selection.Set(start - 1, end - 1);
					}
					else
					{
						m_selection.SetAndReplace(prefixLength, prefixLength, m_negativeSign.ToString());
						m_selection.Set(start + 1, end + 1);
					}
		
					return;
				}
			}
		
			// Check if it's a decimal point (only one is allowed).
			else if (c == m_decimalPoint && m_maxDecimalPlaces > 0)
			{
				if (decimalPos >= 0)
				{
					// Check if we're replacing the decimal point
					if (decimalPos >= start && decimalPos < end)
						m_previousSeparatorCount = separatorCount;
					else
					{	// Otherwise, put the caret on it
						m_selection.Set(decimalPos + 1, decimalPos + 1);
						return;
					}
				}
				else
					m_previousSeparatorCount = separatorCount;
			}
		
			// Check if it's a digit
			else if (Char.IsDigit(c))
			{
				// Check if we're on the right of the decimal point.
				if (decimalPos >= 0 && decimalPos < start)
				{
					if (numericText.Substring(numericDecimalPos + 1).Length == m_maxDecimalPlaces)
					{
						if (start <= decimalPos + m_maxDecimalPlaces)
						{
							end = (end == length ? end : (start + 1));
							m_selection.SetAndReplace(start, end, c.ToString());
						}
						return;
					}
				}
		
				// We're on the left side of the decimal point
				else 
				{
					bool isNegative = (numericText.Length != 0 && numericText[0] == m_negativeSign);
		
					// Make sure we can still enter digits.
					if (start == m_maxWholeDigits + separatorCount + prefixLength + (isNegative ? 1 : 0))
					{
						if (AddDecimalAfterMaxWholeDigits && m_maxDecimalPlaces > 0)
						{
							end = (end == length ? end : (start + 2));
							m_selection.SetAndReplace(start, end, m_decimalPoint.ToString() + c);
						}
						
						return;
					}
		
					if (numericText.Substring(0, numericDecimalPos >= 0 ? numericDecimalPos : numericLen).Length == m_maxWholeDigits + (isNegative ? 1 : 0))
					{
						if (text[start] == m_groupSeparator)
							start++;
	
						end = (end == length ? end : (start + 1));
						m_selection.SetAndReplace(start, end, c.ToString());
						return;
					}
		
					m_previousSeparatorCount = separatorCount;
				}
			}
		
			// Check if it's a non-printable character, such as Backspace or Ctrl+C
			else if (Char.IsControl(c))
				m_previousSeparatorCount = separatorCount;
			else
				return;

			base.HandleKeyPress(sender, e);
		}		
		
		/// <summary>
		///   Handles changes in the textbox text. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's TextChanged event. 
		///   Here it is used to adjust the selection if new separators have been added or removed. </remarks>
		/// <seealso cref="Control.TextChanged" />
		// Fires the TextChanged event if the text is valid.
		protected override void HandleTextChanged(object sender, EventArgs e)
		{			
			TraceLine("NumericBehavior.HandleTextChanged");
			
			Selection.Saver savedSelection = new Selection.Saver(m_textBox);  // save the selection before the text changes
			bool textChangedByKeystroke = m_textChangedByKeystroke;
			base.HandleTextChanged(sender, e);
			
			// Check if the user has changed the number enough to cause
			// one or more separators to be added/removed, in which case
			// the selection may need to be adjusted.
			if (m_previousSeparatorCount >= 0)
			{
				using (savedSelection)
				{		
					int newSeparatorCount = GetGroupSeparatorCount(m_textBox.Text);
					if (m_previousSeparatorCount != newSeparatorCount && savedSelection.Start > m_prefix.Length)
						savedSelection.MoveBy(newSeparatorCount - m_previousSeparatorCount);					
				}
			}				

			// If the text wasn't changed by a keystroke and the UseLostFocusFlagsWhenTextPropertyIsSet flag is set,
			// call the LostFocus handler to adjust the value according to whatever LostFocus flags are set.
			if (HasFlag((int)LostFocusFlag.CallHandlerWhenTextChanges) || 
			   (!textChangedByKeystroke && HasFlag((int)LostFocusFlag.CallHandlerWhenTextPropertyIsSet)))
				HandleLostFocus(sender, e);
			    
			m_textChangedByKeystroke = false;			    
		}		

		/// <summary>
		///   Handles when the control has lost its focus. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's LostFocus event. 
		///   Here it checks the value's against the allowed range and adds any missing zeros. </remarks>
		/// <seealso cref="Control.LostFocus" />
		protected override void HandleLostFocus(object sender, EventArgs e)
		{			
			TraceLine("NumericBehavior.HandleLostFocus");
				
			if (!HasFlag((int)LostFocusFlag.Max))
				return;

			string originalText = GetNumericText(m_textBox.Text, true);
			string text = originalText;
			int length = text.Length;
		
			// If desired, remove any extra leading zeros but always leave one in front of the decimal point
			if (HasFlag((int)LostFocusFlag.RemoveExtraLeadingZeros) && length > 0)
			{
				bool isNegative = (text[0] == m_negativeSign);
				if (isNegative)
					text = text.Substring(1);
				text = text.TrimStart('0');
				if (text == "" || text[0] == m_decimalPoint)
					text = '0' + text;
				if (isNegative)
					text = m_negativeSign + text;
			}
			// Check if the value is empty and we don't want to touch it
			else if (length == 0 && HasFlag((int)LostFocusFlag.DontPadWithZerosIfEmpty))
				return;
		
			int decimalPos = text.IndexOf('.');
			int maxDecimalPlaces = m_maxDecimalPlaces;
			int maxWholeDigits = m_maxWholeDigits;
		
			// Check if we need to pad the number with zeros after the decimal point
			if (HasFlag((int)LostFocusFlag.PadWithZerosAfterDecimal) && maxDecimalPlaces > 0)
			{
				if (decimalPos < 0)
				{
					if (length == 0 || text == "-")
					{
						text = "0";
						length = 1;
					}
					text += '.';
					decimalPos = length++;
				}
		
				text = InsertZeros(text, -1, maxDecimalPlaces - (length - decimalPos - 1));
			}
		
			// Check if we need to pad the number with zeros before the decimal point
			if (HasFlag((int)LostFocusFlag.PadWithZerosBeforeDecimal) && maxWholeDigits > 0)
			{
				if (decimalPos < 0)
					decimalPos = length;
		
				if (length > 0 && text[0] == '-')
					decimalPos--;
		
				text = InsertZeros(text, (length > 0 ? (text[0] == '-' ? 1 : 0) : -1), maxWholeDigits - decimalPos);
			}
		
			if (text != originalText)
			{
				if (decimalPos >= 0 && m_decimalPoint != '.')
					text = text.Replace('.', m_decimalPoint);

				// remember the current selection 
				using (Selection.Saver savedSelection = new Selection.Saver(m_textBox))
				{
					m_textBox.Text = text;
				}
			}		
		}

		/// <summary>
		///   Handles when the control's text gets parsed to be converted to the type expected by the 
		///   object that it's bound to. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method checks if the control's text is empty and if so sets the value to DBNull.Value;
		///   otherwise it converts it to a simple numeric value (without any prefix). </remarks>
		/// <seealso cref="Behavior.HandleBindingChanges" />
		/// <seealso cref="Binding.Parse" />
		protected override void HandleBindingParse(object sender, ConvertEventArgs e)
		{
			if (e.Value.ToString() == "")
				e.Value = DBNull.Value;
			else
				e.Value = GetNumericText(e.Value.ToString(), false);
		}			
	}


	/////////////////////////////////////////////////////////////////////////////
	// Integer behavior
	
	/// <summary>
	///   Behavior class which only allows integer values to be entered. </summary>
	/// <remarks>
	///   This is just a <see cref="NumericBehavior" /> class which maintains
	///   <see cref="MaxDecimalPlaces" /> always at 0. </remarks>
	public class IntegerBehavior : NumericBehavior
	{
		/// <summary>
		///   Initializes a new instance of the IntegerBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor sets <see cref="NumericBehavior.MaxWholeDigits" /> = 9, <see cref="NumericBehavior.MaxDecimalPlaces" /> = 0, <see cref="NumericBehavior.DigitsInGroup" /> = 0, 
		///   <see cref="NumericBehavior.Prefix" /> = "", <see cref="NumericBehavior.AllowNegative" /> = true, and the rest of the properties according to user's system. </remarks>
		public IntegerBehavior(TextBoxBase textBox) : 
			base(textBox, 9, 0)
		{			
			SetDefaultRange();
		}

		/// <summary>
		///   Initializes a new instance of the IntegerBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <param name="maxWholeDigits">
		///   The maximum number of digits allowed to the left of the decimal point. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor sets <see cref="NumericBehavior.MaxDecimalPlaces" /> = 0, <see cref="NumericBehavior.DigitsInGroup" /> = 0, <see cref="NumericBehavior.Prefix" /> = "", <see cref="NumericBehavior.AllowNegative" /> = true, 
		///   and the rest of the properties according to user's system. </remarks>
		public IntegerBehavior(TextBoxBase textBox, int maxWholeDigits) :
			base(textBox, maxWholeDigits, 0)
		{	
			SetDefaultRange();
		}
		
		/// <summary>
		///   Initializes a new instance of the IntegerBehavior class by copying it from 
		///   another IntegerBehavior object. </summary>
		/// <param name="behavior">
		///   The IntegerBehavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		public IntegerBehavior(IntegerBehavior behavior) :
			base(behavior)
		{
			SetDefaultRange();
		}

		/// <summary>
		///   Changes the default min and max values to 32-bit integer ranges.</summary>
		private void SetDefaultRange()
		{
			RangeMin = Int32.MinValue;
			RangeMax = Int32.MaxValue;		
		}

		/// <summary>
		///   Gets the maximum number of digits allowed right of the decimal point, which is always 0. </summary>
		/// <seealso cref="NumericBehavior.MaxWholeDigits" />
		public new int MaxDecimalPlaces
		{
			get 
			{ 
				return base.MaxDecimalPlaces; 
			}
		}

		/// <summary>
		///   Gets or sets a mask value representative of this object's properties. </summary>
		/// <remarks>
		///   This property behaves like <see cref="NumericBehavior.Mask" /> except that  
		///   <see cref="NumericBehavior.MaxDecimalPlaces" /> is maintained with a value of 0. </remarks>
		public new string Mask
		{
			get 
			{ 
				return base.Mask; 
			}
			set
			{
				base.Mask = value;
				if (base.MaxDecimalPlaces > 0)
					base.MaxDecimalPlaces = 0;
			}
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	// Currency behavior
	
	/// <summary>
	///   Behavior class useful for entering monetary values. </summary>
	/// <remarks>
	///   This is just a <see cref="NumericBehavior" /> class customized to make the value look monetary. 
	///   It sets the <see cref="NumericBehavior.Prefix" /> to the currency sign specified in the user's system (such as a '$'). 
	///   It also separates thousands using the character specified in the system (such as a comma). 
	///   It sets <see cref="NumericBehavior.MaxDecimalPlaces" /> to whatever is specified in the system -- usually two. </remarks>
	public class CurrencyBehavior : NumericBehavior
	{
		/// <summary>
		///   Initializes a new instance of the CurrencyBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor sets <see cref="NumericBehavior.MaxWholeDigits" /> = 9, <see cref="NumericBehavior.AllowNegative" /> = true, 
		///   and the rest of the properties according to user's system. If the system has the 
		///   currency symbol configured to be placed in front of the value, then it is assigned to the <see cref="NumericBehavior.Prefix" />.
		///   Also, the number is automatically padded with zeros after the <see cref="NumericBehavior.DecimalPoint" /> when the textbox
		///   loses focus. </remarks>
		public CurrencyBehavior(TextBoxBase textBox) :
			base(textBox)
		{
			m_flags |= ((int)LostFocusFlag.RemoveExtraLeadingZeros | 
			            (int)LostFocusFlag.PadWithZerosAfterDecimal | 
			            (int)LostFocusFlag.DontPadWithZerosIfEmpty | 
			            (int)LostFocusFlag.CallHandlerWhenTextPropertyIsSet);

			// Get the system's current settings
			DigitsInGroup = NumberFormatInfo.CurrentInfo.CurrencyGroupSizes[0];
			MaxDecimalPlaces = NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits;
			DecimalPoint = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator[0];
			GroupSeparator = NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator[0];			

			// Determine how the currency symbol should be shown for the prefix
			switch (NumberFormatInfo.CurrentInfo.CurrencyPositivePattern)
			{
				case 0:		// Prefix, no separation
					Prefix = NumberFormatInfo.CurrentInfo.CurrencySymbol;
					break;
				case 2:		// Prefix, one space separation
					Prefix = NumberFormatInfo.CurrentInfo.CurrencySymbol + ' ';
					break;
				
				// The rest are suffixes, so no prefix
			}

			AdjustDecimalAndGroupSeparators();
		}

		/// <summary>
		///   Initializes a new instance of the CurrencyBehavior class by copying it from 
		///   another CurrencyBehavior object. </summary>
		/// <param name="behavior">
		///   The CurrencyBehavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		public CurrencyBehavior(CurrencyBehavior behavior) :
			base(behavior)
		{
		}
	}			
			

	/////////////////////////////////////////////////////////////////////////////
	// Date behavior

	/// <summary>
	///   Behavior class which handles input of date values in mm/dd/yyyy or dd/mm/yyyy format. </summary>
	/// <remarks>
	///   This behavior is designed to let the user enter a date value quickly and accurately.  
	///   As the user enters the digits, the slashes are automatically filled in. The user may only remove 
	///   characters from the right side of the value entered. This helps to keep the value properly formatted. 
	///   The user may also use the up/down arrow keys to increment/decrement the month, day, or year, depending on the location of the caret.  </remarks>
	/// <seealso cref="TimeBehavior" />
	/// <seealso cref="DateTimeBehavior" />
	public class DateBehavior : Behavior
	{
		// Fields
		private DateTime m_rangeMin = new DateTime(1900, 1, 1);
		private DateTime m_rangeMax = new DateTime(9998, 12, 31);
		private char m_separator = '/';

		/// <summary>
		///   Internal values that are added/removed to the <see cref="Behavior.Flags" /> property by other
		///   properties of this class. </summary>
		[Flags]
		protected enum Flag
		{
			/// <summary> The day is displayed in front of the month. </summary>
			/// <seealso cref="ShowDayBeforeMonth" />
			DayBeforeMonth								= 0x00010000,
		};

		/// <summary>
		///   Initializes a new instance of the DateBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor determines the <see cref="Separator" /> and date format (mm/dd/yyyy or dd/mm/yyyy) from the user's system. </remarks>
		/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
		public DateBehavior(TextBoxBase textBox) :
			this(textBox, true)
		{
		}

		/// <summary>
		///   Initializes a new instance of the DateBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <param name="addEventHandlers">
		///   If true, the textBox's event handlers are tied to the corresponding methods on this behavior object. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor determines the <see cref="Separator" /> and date format (mm/dd/yyyy or dd/mm/yyyy) from the user's system. 
		///   It is meant to be used internally by the DateTime behavior class. </remarks>
		/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
		internal DateBehavior(TextBoxBase textBox, bool addEventHandlers) :
			base(textBox, addEventHandlers)
		{
			// Get the system's date separator
			m_separator = DateTimeFormatInfo.CurrentInfo.DateSeparator[0];

			// Determine if the day should go before the month
			string shortDate = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
			for (int iPos = 0; iPos < shortDate.Length; iPos++)
			{
				char c = Char.ToUpper(shortDate[iPos]);
				if (c == 'M')	// see if the month is first
					break;
				if (c == 'D')	// see if the day is first, and then set the flag
				{
					m_flags |= (int)Flag.DayBeforeMonth;
					break;
				}
			}
		}

		/// <summary>
		///   Initializes a new instance of the DateBehavior class by copying it from 
		///   another DateBehavior object. </summary>
		/// <param name="behavior">
		///   The DateBehavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		public DateBehavior(DateBehavior behavior) :
			base(behavior)
		{
			m_rangeMin = behavior.m_rangeMin;
			m_rangeMax = behavior.m_rangeMax;
			m_separator = behavior.m_separator;
		}

		/// <summary>
		///   Calls either the <see cref="HandleKeyPress" /> or <see cref="HandleKeyDown" /> method. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is designed to be called by the <see cref="DateTimeBehavior" />
		///   class since it does not have public access to HandleKeyPress or HandleKeyDown.
		///   The type of EventArgs determines which method is called. </remarks>
		/// <seealso cref="Control.KeyDown" />
		internal void HandleKeyEvent(object sender, EventArgs e)
		{
			if (e is KeyEventArgs)
				HandleKeyDown(sender, (KeyEventArgs)e);
			else if (e is KeyPressEventArgs)
				HandleKeyPress(sender, (KeyPressEventArgs)e);
		}
		
		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyDown event. </remarks>
		/// <seealso cref="Control.KeyDown" />
		protected override void HandleKeyDown(object sender, KeyEventArgs e)
		{
			TraceLine("DateBehavior.HandleKeyDown " + e.KeyCode);	

			// Check to see if it's read only
			if (m_textBox.ReadOnly)
				return;
								
			e.Handled = true;

			switch (e.KeyCode)
			{
				case Keys.Delete:
				{
					// If deleting make sure it's the last character or that
					// the selection goes all the way to the end of the text

					int start, end;
					m_selection.Get(out start, out end);

					string text = m_textBox.Text;
					int length = text.Length;

					if (end != length)
					{
						if (!(end == start && end == length - 1))
							return;
					}

					m_noTextChanged = true;
					break;
				}

				case Keys.Up:
				{
					// If pressing the UP arrow, increment the corresponding value.

					int start, end;
					m_selection.Get(out start, out end);

					if (start >= GetYearStartPosition() && start <= GetYearStartPosition() + 4)
					{
						int year = Year;
						if (year >= m_rangeMin.Year && year < m_rangeMax.Year)
							Year = ++year;
					}

					else if (start >= GetMonthStartPosition() && start <= GetMonthStartPosition() + 2)
					{
						int month = Month;
						if (month >= GetMinMonth() && month < GetMaxMonth())
							Month = ++month;
					}

					else if (start >= GetDayStartPosition() && start <= GetDayStartPosition() + 2)
					{
						int day = Day;
						if (day >= GetMinDay() && day < GetMaxDay())
							Day = ++day;
					}
					
					return;
				}

				case Keys.Down:
				{
					// If pressing the DOWN arrow, decrement the corresponding value.

					int start, end;
					m_selection.Get(out start, out end);

					if (start >= GetYearStartPosition() && start <= GetYearStartPosition() + 4)
					{
						int year = Year;
						if (year > m_rangeMin.Year)
							Year = --year;
					}

					else if (start >= GetMonthStartPosition() && start <= GetMonthStartPosition() + 2)
					{
						int month = Month;
						if (month > GetMinMonth())
							Month = --month;
					}

					else if (start >= GetDayStartPosition() && start <= GetDayStartPosition() + 2)
					{
						int day = Day;
						if (day > GetMinDay())
							Day = --day;
					}
					
					return;
				}
			}

			base.HandleKeyDown(sender, e);
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyPress event. </remarks>
		/// <seealso cref="Control.KeyPress" />
		protected override void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			TraceLine("DateBehavior.HandleKeyPress " + e.KeyChar);	
			
			// Check to see if it's read only
			if (m_textBox.ReadOnly)
				return;
					
			char c = e.KeyChar;
			e.Handled = true;
			m_noTextChanged = true;
		
			int start, end;
			m_selection.Get(out start, out end);

			string text = m_textBox.Text;
			int length = text.Length;

			// Check for a non-printable character (such as Ctrl+C)
			if (Char.IsControl(c))
			{
				if (c == (short)Keys.Back && start != length)
				{
					SendKeys.Send("{LEFT}");  // move the cursor left
					return;
				}
				
				// Allow backspace only if the cursor is all the way to the right
				base.HandleKeyPress(sender, e);
				return;
			}

			// Add the digit depending on its location
			switch (start)
			{
				case 0:		// FIRST DIGIT
				{
					if (ShowDayBeforeMonth)
					{
						if (IsValidDayDigit(c, 0))
						{
							if (length > start)
							{
								m_selection.SetAndReplace(start, start + 1, c.ToString());

								if (length > start + 1)
								{
									if (!IsValidDay(Day))
									{
										m_selection.SetAndReplace(start + 1, start + 2, GetMinDayDigit(1).ToString());
										m_selection.Set(start + 1, start + 2);
									}
								}
							}
							else
								base.HandleKeyPress(sender, e);
						}
						// Check if we can insert the digit with a leading zero
						else if (length == start && GetMinDayDigit(0) == '0' && IsValidDayDigit(c, 1))
							m_selection.SetAndReplace(start, start + 2, "0" + c);					
					}
					else
					{
						if (IsValidMonthDigit(c, 0))
						{
							if (length > start)
							{
								m_selection.SetAndReplace(start, start + 1, c.ToString());

								if (length > start + 1)
								{
									if (!IsValidMonth(Month))
									{
										m_selection.SetAndReplace(start + 1, start + 2, GetMinMonthDigit(1).ToString());
										m_selection.Set(start + 1, start + 2);
									}
								}
								AdjustMaxDay();
							}
							else
								base.HandleKeyPress(sender, e);
						}
						// Check if we can insert the digit with a leading zero
						else if (length == start && GetMinMonthDigit(0) == '0' && IsValidMonthDigit(c, 1))
							m_selection.SetAndReplace(start, start + 2, "0" + c);					
					}
					break;
				}
				case 1:		// SECOND DIGIT
				{
					if (ShowDayBeforeMonth)
					{
						if (IsValidDayDigit(c, 1))
						{
							if (length > start)
								m_selection.SetAndReplace(start, start + 1, c.ToString());
							else
								base.HandleKeyPress(sender, e);
						}
						// Check if it's a slash and the first digit (preceded by a zero) is a valid month
						else if (c == m_separator && length == start && GetMinDayDigit(0) == '0' && IsValidDay(ToInt("0" + text[0])))
							m_selection.SetAndReplace(0, start, "0" + text[0] + c);					
					}
					else
					{
						if (IsValidMonthDigit(c, 1))
						{
							if (length > start)
							{
								m_selection.SetAndReplace(start, start + 1, c.ToString());

								if (Day > 0 && AdjustMaxDay())
									m_selection.Set(GetDayStartPosition(), GetDayStartPosition() + 2);
							}
							else
								base.HandleKeyPress(sender, e);
						}				
						// Check if it's a slash and the first digit (preceded by a zero) is a valid month
						else if (c == m_separator && length == start && GetMinMonthDigit(0) == '0' && IsValidMonth(ToInt("0" + text[0])))
							m_selection.SetAndReplace(0, start, "0" + text[0] + c);					
					}
					break;
				}
				
				case 2:		// FIRST SLASH
				{
					int slash = 0;
					if (c == m_separator)
						slash = 1;
					else
					{
						if (ShowDayBeforeMonth)
							slash = (IsValidMonthDigit(c, 0) || (length == start && GetMinMonthDigit(0) == '0' && IsValidMonthDigit(c, 1))) ? 2 : 0;
						else
							slash = (IsValidDayDigit(c, 0) || (length == start && GetMinDayDigit(0) == '0' && IsValidDayDigit(c, 1))) ? 2 : 0;
					}

					// If we need the slash, enter it
					if (slash != 0)
						m_selection.SetAndReplace(start, start + 1, m_separator.ToString());

					// If the slash is to be preceded by a valid digit, "type" it in.
					if (slash == 2)
						SendKeys.Send(c.ToString());
					break;
				}

				case 3:		// THIRD DIGIT
				{
					if (ShowDayBeforeMonth)
					{
						if (IsValidMonthDigit(c, 0))
						{
							if (length > start)
							{
								m_selection.SetAndReplace(start, start + 1, c.ToString());

								if (length > start + 1)
								{
									if (!IsValidMonth(Month))
									{
										m_selection.SetAndReplace(start + 1, start + 2, GetMinMonthDigit(1).ToString());
										m_selection.Set(start + 1, start + 2);
									}
								}
							}
							else
								base.HandleKeyPress(sender, e);

							AdjustMaxDay();
						}
						// Check if we can insert the digit with a leading zero
						else if (length == start && GetMinMonthDigit(0) == '0' && IsValidMonthDigit(c, 1))
						{
							m_selection.SetAndReplace(start, start + 2, "0" + c);					
							AdjustMaxDay();
						}
					}
					else
					{
						if (IsValidDayDigit(c, 0))
						{
							if (length > start)
							{
								m_selection.SetAndReplace(start, start + 1, c.ToString());

								if (length > start + 1)
								{
									if (!IsValidDay(Day))
									{
										m_selection.SetAndReplace(start + 1, start + 2, GetMinDayDigit(1).ToString());
										m_selection.Set(start + 1, start + 2);
									}
								}
							}
							else
								base.HandleKeyPress(sender, e);
						}
						// Check if we can insert the digit with a leading zero
						else if (length == start && GetMinDayDigit(0) == '0' && IsValidDayDigit(c, 1))
							m_selection.SetAndReplace(start, start + 2, "0" + c);					
					}
					break;			
				}

				case 4:		// FOURTH DIGIT
				{
					if (ShowDayBeforeMonth)
					{
						if (IsValidMonthDigit(c, 1))
						{
							if (length > start)
							{
								m_selection.SetAndReplace(start, start + 1, c.ToString());

								if (Day > 0 && AdjustMaxDay())
									m_selection.Set(GetDayStartPosition(), GetDayStartPosition() + 2);
							}
							else
							{
								base.HandleKeyPress(sender, e);
								AdjustMaxDay();
							}
						}
						// Check if it's a slash and the first digit (preceded by a zero) is a valid month
						else if (c == m_separator && length == start && GetMinMonthDigit(0) == '0' && IsValidMonth(ToInt("0" + text[3])))
							m_selection.SetAndReplace(3, start, "0" + text[3] + c);					
					}
					else
					{
						if (IsValidDayDigit(c, 1))
						{
							if (length > start)
								m_selection.SetAndReplace(start, start + 1, c.ToString());
							else
								base.HandleKeyPress(sender, e);
						}
						// Check if it's a slash and the first digit (preceded by a zero) is a valid month
						else if (c == m_separator && length == start && GetMinDayDigit(0) == '0' && IsValidDay(ToInt("0" + text[3])))
							m_selection.SetAndReplace(3, start, "0" + text[3] + c);					
					}
					break;
				}

				case 5:		// SECOND SLASH	(year's first digit)
				{
					int slash = 0;
					if (c == m_separator)
						slash = 1;
					else
						slash = (IsValidYearDigit(c, 0) ? 2 : 0);

					// If we need the slash, enter it
					if (slash != 0)
						m_selection.SetAndReplace(start, start + 1, m_separator.ToString());

					// If the slash is to be preceded by a valid digit, "type" it in.
					if (slash == 2)
						SendKeys.Send(c.ToString());
					break;			
				}

				case 6:		// YEAR (all 4 digits)
				case 7:
				case 8:
				case 9:
				{
					if (IsValidYearDigit(c, start - GetYearStartPosition()))
					{
						if (length > start)
						{
							m_selection.SetAndReplace(start, start + 1, c.ToString());

							for (; start + 1 < length && start < 9; start++)
							{
								if (!IsValidYearDigit(text[start + 1], start - (GetYearStartPosition() - 1)))
								{
									m_selection.Set(start + 1, 10);
									StringBuilder portion = new StringBuilder();
									for (int iPos = start + 1; iPos < length && iPos < 10; iPos++)
										portion.Append(GetMinYearDigit(iPos - GetYearStartPosition(), false));
									
									m_selection.Replace(portion.ToString());
									m_selection.Set(start + 1, 10);
									break;
								}
							}
						}
						else
							base.HandleKeyPress(sender, e);

						if (IsValidYear(Year))
						{
							AdjustMaxDay();			// adjust the day first
							AdjustMaxMonthAndDay();	// then adjust the month and the day, if necessary
						}
					}
					break;
				}
			}
		}

		/// <summary>
		///   Converts an integer value to a 2-digit string (00 - 99). </summary>
		/// <param name="value">
		///   The number to convert. </param>
		/// <returns>
		///   The return value is the formatted string. </returns>
		/// <remarks>
		///   This is convenience method for formatting 2 digit
		///   values such as the month and day. </remarks>
		protected static string TwoDigits(int value)
		{
			return String.Format("{0,2:00}", value);
		}

		/// <summary>
		///   Retrieves the zero-based position of the month inside the texbox. </summary>
		/// <returns>
		///   The return value is the starting position of the month. </returns>
		/// <remarks>
		///   This is based on whether the month is shown before or after the day. </remarks>
		protected int GetMonthStartPosition()
		{
			return ShowDayBeforeMonth ? 3 : 0;
		}

		/// <summary>
		///   Retrieves the zero-based position of the day inside the texbox. </summary>
		/// <returns>
		///   The return value is the starting position of the day. </returns>
		/// <remarks>
		///   This is based on whether the day is shown before or after the month. </remarks>
		protected int GetDayStartPosition()
		{
			return ShowDayBeforeMonth ? 0 : 3;
		}

		/// <summary>
		///   Retrieves the zero-based position of the year inside the texbox. </summary>
		/// <returns>
		///   The return value is the starting position of the year. </returns>
		/// <remarks>
		///   This is always 6. </remarks>
		protected int GetYearStartPosition()
		{
			return 6;
		}

		/// <summary>
		///   Retrieves the maximum value for the month based on the year and the allowed range. </summary>
		/// <returns>
		///   The return value is the maximum value for the month. </returns>
		protected int GetMaxMonth()
		{
			if (GetValidYear() == m_rangeMax.Year)
				return m_rangeMax.Month;
			return 12;
		}

		/// <summary>
		///   Retrieves the minimum value for the month based on the year and the allowed range. </summary>
		/// <returns>
		///   The return value is the minimum value for the month. </returns>
		protected int GetMinMonth()
		{
			if (GetValidYear() == m_rangeMin.Year)
				return m_rangeMin.Month;
			return 1;
		}

		/// <summary>
		///   Retrieves the maximum value for the day based on the month, year, and the allowed range. </summary>
		/// <returns>
		///   The return value is the maximum value for the day. </returns>
		protected int GetMaxDay()
		{
			int year = GetValidYear();
			int month = GetValidMonth();

			if (year == m_rangeMax.Year && month == m_rangeMax.Month)
				return m_rangeMax.Day;

			return GetMaxDayOfMonth(month, year);
		}

		/// <summary>
		///   Retrieves the minimum value for the day based on the month, year, and the allowed range. </summary>
		/// <returns>
		///   The return value is the minimum value for the day. </returns>
		protected int GetMinDay()
		{
			int year = GetValidYear();
			int month = GetValidMonth();

			if (year == m_rangeMin.Year && month == m_rangeMin.Month)
				return m_rangeMin.Day;

			return 1;
		}

		/// <summary>
		///   Retrieves the maximum day for a given month and year. </summary>
		/// <param name="month">
		///   The month (1 - 12). </param>
		/// <param name="year">
		///   The year (1900 - 9999). </param>
		/// <returns>
		///   The return value is the maximum day (1 - 31). </returns>
		protected static int GetMaxDayOfMonth(int month, int year)
		{
			Debug.Assert(month >= 1 && month <= 12);

			switch (month)
			{
				case 4:
				case 6:
				case 9:
				case 11:
					return 30;

				case 2:
					return DateTime.IsLeapYear(year) ? 29 : 28;
			}
			return 31;
		}

		/// <summary>
		///   Retrieves the maximum digit that a month value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the month (0 or 1). </param>
		/// <returns>
		///   The return value is the maximum digit that it can be. </returns>
		protected char GetMaxMonthDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);

			int year = GetValidYear();
			int maxMonth = m_rangeMax.Month;
			int maxYear = m_rangeMax.Year;

			// First digit
			if (position == 0)
			{
				// If the year is at the max, then use the first digit of the max month
				if (year == maxYear)
					return TwoDigits(maxMonth)[0];

				// Otherwise, it's always '1'
				return '1';
			}

			// Second digit
			string text = m_textBox.Text;
			char firstDigit = (text.Length > GetMonthStartPosition()) ? text[GetMonthStartPosition()] : '0';
			Debug.Assert(firstDigit != 0);  // must have a valid first digit at this point

			// If the year is at the max, then check if the first digits match
			if (year == maxYear && (IsValidYear(Year) || maxYear == m_rangeMin.Year))
			{
				// If the first digits match, then use the second digit of the max month
				if (TwoDigits(maxMonth)[0] == firstDigit)
					return TwoDigits(maxMonth)[1];

				// Assuming the logic for the first digit is correct, then it must be '0'
				Debug.Assert(firstDigit == '0');
				return '9';  
			}

			// Use the first digit to determine the second digit's max
			return (firstDigit == '1' ? '2' : '9');
		}

		/// <summary>
		///   Retrieves the minimum digit that a month value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the month (0 or 1). </param>
		/// <returns>
		///   The return value is the minimum digit that it can be. </returns>
		protected char GetMinMonthDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);

			int year = GetValidYear();
			int minMonth = m_rangeMin.Month;
			int minYear = m_rangeMin.Year;

			// First digit
			if (position == 0)
			{
				// If the year is at the min, then use the first digit of the min month
				if (year == minYear)
					return TwoDigits(minMonth)[0];

				// Otherwise, it's always '0'
				return '0';
			}

			// Second digit
			string text = m_textBox.Text;
			char firstDigit = (text.Length > GetMonthStartPosition()) ? text[GetMonthStartPosition()] : '0';
			if (firstDigit == 0)
				return '1';

			// If the year is at the max, then check if the first digits match
			if (year == minYear && (IsValidYear(Year) || minYear == m_rangeMax.Year))
			{
				// If the first digits match, then use the second digit of the max month
				if (TwoDigits(minMonth)[0] == firstDigit)
					return TwoDigits(minMonth)[1];

				return '0';  
			}

			// Use the first digit to determine the second digit's min
			return (firstDigit == '1' ? '0' : '1');
		}

		/// <summary>
		///   Checks if a digit is valid for the month at one of its two character positions. </summary>
		/// <param name="c">
		///   The digit to check. </param>
		/// <param name="position">
		///   The position of the digit of the month (0 or 1). </param>
		/// <returns>
		///   If the digit is valid for the month (at the given position), the return value is true; otherwise it is false. </returns>
		protected bool IsValidMonthDigit(char c, int position)
		{
			return (c >= GetMinMonthDigit(position) && c <= GetMaxMonthDigit(position));
		}

		/// <summary>
		///   Checks if a month is valid -- falls within the allowed range. </summary>
		/// <param name="month">
		///   The month to check. </param>
		/// <returns>
		///   If the month falls within the allowed range, the return value is true; otherwise it is false. </returns>
		protected bool IsValidMonth(int month)
		{
			int year = GetValidYear();
			int day = GetValidDay();
			try
			{
				return IsWithinRange(new DateTime(year, month, day));
			}
			catch
			{
				return false;
			}					
		}

		/// <summary>
		///   Retrieves the maximum digit that a day value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the day (0 or 1). </param>
		/// <returns>
		///   The return value is the maximum digit that it can be. </returns>
		protected char GetMaxDayDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);

			int month = GetValidMonth();
			int year = GetValidYear();
			int maxDay = m_rangeMax.Day;
			int maxMonth = m_rangeMax.Month;
			int maxYear = m_rangeMax.Year;

			// First digit
			if (position == 0)
			{
				// If the year and month are at the max, then use the first digit of the max day
				if (year == maxYear && month == maxMonth)
					return TwoDigits(maxDay)[0];
				return TwoDigits(GetMaxDayOfMonth(month, year))[0];
			}

			// Second digit
			string text = m_textBox.Text;
			char firstDigit = (text.Length > GetDayStartPosition()) ? text[GetDayStartPosition()] : '0';
			Debug.Assert(firstDigit != 0);  // must have a valid first digit at this point

			// If the year and month are at the max, then use the second digit of the max day
			if (year == maxYear && month == maxMonth && TwoDigits(maxDay)[0] == firstDigit)
				return TwoDigits(maxDay)[1];

			if (firstDigit == '0' || 
				firstDigit == '1' || 
				(firstDigit == '2' && month != 2) || 
				(month == 2 && !IsValidYear(Year)))
				return '9';
			return TwoDigits(GetMaxDayOfMonth(month, year))[1];
		}

		/// <summary>
		///   Retrieves the minimum digit that a day value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the day (0 or 1). </param>
		/// <returns>
		///   The return value is the minimum digit that it can be. </returns>
		protected char GetMinDayDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);

			int month = GetValidMonth();
			int year = GetValidYear();
			int minDay = m_rangeMin.Day;
			int minMonth = m_rangeMin.Month;
			int minYear = m_rangeMin.Year;

			// First digit
			if (position == 0)
			{
				// If the year and month are at the min, then use the first digit of the min day
				if (year == minYear && month == minMonth)
					return TwoDigits(minDay)[0];
				return '0';
			}

			// Second digit
			string text = m_textBox.Text;
			char firstDigit = (text.Length > GetDayStartPosition()) ? text[GetDayStartPosition()] : '0';
			if (firstDigit == 0)  // must have a valid first digit at this point
				return '1';

			// If the year and month are at the max, then use the first second of the max day
			if (year == minYear && month == minMonth && TwoDigits(minDay)[0] == firstDigit)
				return TwoDigits(minDay)[1];

			// Use the first digit to determine the second digit's min
			return (firstDigit == '0' ? '1' : '0');
		}

		/// <summary>
		///   Checks if a digit is valid for the day at one of its two character positions. </summary>
		/// <param name="c">
		///   The digit to check. </param>
		/// <param name="position">
		///   The position of the digit of the day (0 or 1). </param>
		/// <returns>
		///   If the digit is valid for the day (at the given position), the return value is true; otherwise it is false. </returns>
		protected bool IsValidDayDigit(char c, int position)
		{
			return (c >= GetMinDayDigit(position) && c <= GetMaxDayDigit(position));
		}

		/// <summary>
		///   Checks if a day is valid -- falls within the allowed range. </summary>
		/// <param name="day">
		///   The day to check. </param>
		/// <returns>
		///   If the day falls within the allowed range, the return value is true; otherwise it is false. </returns>
		protected bool IsValidDay(int day)
		{
			try
			{
				return IsWithinRange(new DateTime(GetValidYear(), GetValidMonth(), day));
			}
			catch
			{
				return false;
			}					
		}

		/// <summary>
		///   Checks if a year is valid -- falls within the allowed range. </summary>
		/// <param name="year">
		///   The year to check. </param>
		/// <returns>
		///   If the year falls within the allowed range, the return value is true; otherwise it is false. </returns>
		protected bool IsValidYear(int year)
		{
			return (year >= m_rangeMin.Year && year <= m_rangeMax.Year);
		}

		/// <summary>
		///   Adjusts the month (to the minimum) if not valid; otherwise adjusts the day (to the maximum) if not valid. </summary>
		/// <returns>
		///   If the month and/or day gets adjusted, the return value is true; otherwise it is false. </returns>
		protected bool AdjustMaxMonthAndDay()
		{
			int month = Month;	
			if (month != 0 && !IsValidMonth(month))
			{
				Month = GetMinMonth();  // this adjusts the day automatically
				return true;
			}

			return AdjustMaxDay();
		}

		/// <summary>
		///   Adjusts the day (to the maximum) if not valid. </summary>
		/// <returns>
		///   If the day gets adjusted, the return value is true; otherwise it is false. </returns>
		protected bool AdjustMaxDay()
		{
			int day = Day;
			if (day != 0 && !IsValidDay(day))
			{
				Day = GetMaxDay();
				return true;
			}
			
			return false;	// nothing had to be adjusted
		}

		/// <summary>
		///   Retrieves the maximum digit that a year value can take, at one of its four character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the day (0 to 3). </param>
		/// <returns>
		///   The return value is the maximum digit that it can be. </returns>
		protected char GetMaxYearDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 3);

			string yearStr = "" + Year;
			string maxYear = "" + m_rangeMax.Year;

			if (position == 0 || ToInt(maxYear.Substring(0, position)) <= ToInt(yearStr.Substring(0, position)))
				return maxYear[position];
			return '9';
		}

		/// <summary>
		///   Retrieves the minimum digit that a year value can take, at one of its four character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the day (0 to 3). </param>
		/// <param name="validYear">
		///   If true, a valid year is used if the current one is not. </param>
		/// <returns>
		///   The return value is the minimum digit that it can be. </returns>
		protected char GetMinYearDigit(int position, bool validYear)
		{
			Debug.Assert(position >= 0 && position <= 3);

			int year = Year;
			if (validYear && !IsValidYear(year))
				year = GetValidYear();

			string yearStr = "" + year;
			string minYear = "" + m_rangeMin.Year;
			
			if (position == 0 || ToInt(minYear.Substring(0, position)) >= ToInt(yearStr.Substring(0, position)))
				return minYear[position];
			return '0';
		}

		/// <summary>
		///   Checks if a digit is valid for the year at one of its four character positions. </summary>
		/// <param name="c">
		///   The digit to check. </param>
		/// <param name="position">
		///   The position of the digit of the day (0 to 3). </param>
		/// <returns>
		///   If the digit is valid for the year (at the given position), the return value is true; otherwise it is false. </returns>
		protected bool IsValidYearDigit(char c, int position)
		{
			return (c >= GetMinYearDigit(position, false) && c <= GetMaxYearDigit(position));
		}

		/// <summary>
		///   Retrieves the month on the textbox as a valid value. </summary>
		/// <returns>
		///   The return value is the valid value for the month (1 - 12). </returns>
		/// <remarks>
		///   The method checks the value of the month on the textbox.  
		///   If it is within the allowed range, it returns it.  
		///   If it is less than the minimum allowed, the minimum is returned.
		///   If it is more than the maximum allowed, the maximum is returned. </remarks>
		protected int GetValidMonth()
		{
			int month = Month;
			
			// It it's outside the range, fix it
			if (month < GetMinMonth())
				month = GetMinMonth();
			else if (month > GetMaxMonth())
				month = GetMaxMonth();

			return month;
		}

		/// <summary>
		///   Retrieves the day on the textbox as a valid value. </summary>
		/// <returns>
		///   The return value is the valid value for the day (1 - 31). </returns>
		/// <remarks>
		///   The method checks the value of the day on the textbox.  
		///   If it is within the allowed range, it returns it.  
		///   If it is less than the minimum allowed, the minimum is returned.
		///   If it is more than the maximum allowed, the maximum is returned. </remarks>
		protected int GetValidDay()
		{
			int day = Day;

			// It it's outside the range, fix it
			if (day < GetMinDay())
				day = GetMinDay();
			else if (day > GetMaxDay())
				day = GetMaxDay();

			return day;
		}

		/// <summary>
		///   Retrieves the year on the textbox as a valid value. </summary>
		/// <returns>
		///   The return value is the valid value for the year. </returns>
		/// <remarks>
		///   The method checks the value of the year on the textbox.  
		///   If it is within the allowed range, it returns it.  
		///   If it is less than the minimum allowed, the minimum is returned.
		///   If it is more than the maximum allowed, the maximum is returned. </remarks>
		protected int GetValidYear()
		{
			int year = Year;
			if (year < m_rangeMin.Year)
			{
				year = DateTime.Today.Year;
				if (year < m_rangeMin.Year)
					year = m_rangeMin.Year;
			}
			if (year > m_rangeMax.Year)
				year = m_rangeMax.Year;

			return year;
		}			

		/// <summary>
		///   Gets or sets the month on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid month. </exception>
		/// <remarks>
		///   If the month is not valid on the textbox, this property will return 0.
		///   This property must be set with a month that falls within the allowed range. </remarks>
		/// <seealso cref="Day" />
		/// <seealso cref="Year" />
		public int Month
		{
			get 
			{
				string text = m_textBox.Text;

				int startPos = GetMonthStartPosition();
				int slash = text.IndexOf(m_separator);

				if (startPos != 0 && slash > 0)
					startPos = slash + 1;

				if (text.Length >= startPos + 2)
					return ToInt(text.Substring(startPos, 2));
				return 0;
			}
			set
			{					
				using (Selection.Saver savedSelection = new Selection.Saver(m_textBox)) 	// remember the current selection
				{						
					if (Month > 0)		// see if there's already a month
						m_selection.Set(GetMonthStartPosition(), GetMonthStartPosition() + 3);

					m_selection.Replace(TwoDigits(value) + m_separator);	// set the month

					AdjustMaxDay();	// adjust the day if it's out of range

					// Verify it's in range
					if (!IsValidMonth(value))
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>
		///   Gets or sets the day on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid day. </exception>
		/// <remarks>
		///   If the day is not valid on the textbox, this property will return 0.
		///   This property must be set with a day that falls within the allowed range. </remarks>
		/// <seealso cref="Month" />
		/// <seealso cref="Year" />
		public int Day
		{
			get
			{
				string text = m_textBox.Text;

				int startPos = GetDayStartPosition();
				int slash = text.IndexOf(m_separator);

				if (startPos != 0 && slash > 0)
					startPos = slash + 1;

				if (text.Length >= startPos + 2)
					return ToInt(text.Substring(startPos, 2));
				return 0;
			}
			set
			{
				// Verify it's in range
				if (!IsValidDay(value))
					throw new ArgumentOutOfRangeException();

				using (Selection.Saver savedSelection = new Selection.Saver(m_textBox))	// remember the current selection
				{
					if (Day > 0)		// see if there's already a day
						m_selection.Set(GetDayStartPosition(), GetDayStartPosition() + 3);

					m_selection.Replace(TwoDigits(value) + m_separator);	// set the day
				}
			}
		}
		
		/// <summary>
		///   Gets or sets the year on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid year. </exception>
		/// <remarks>
		///   If the year is not valid on the textbox, this property will return 0.
		///   This property must be set with a year that falls within the allowed range. </remarks>
		/// <seealso cref="Month" />
		/// <seealso cref="Day" />
		public int Year
		{
			get
			{
				string text = m_textBox.Text;
				int length = text.Length;

				int slash = text.LastIndexOf(m_separator);
				if (slash > 0 && slash < length - 1)
					return ToInt(text.Substring(slash + 1, Math.Min(4, length - slash - 1)));
				return 0;
			}
			set
			{
				// Verify it's in range
				if (!IsValidYear(value))
					throw new ArgumentOutOfRangeException();

				using (Selection.Saver savedSelection = new Selection.Saver(m_textBox))	// remember the current selection
				{
					if (Year > 0)		// see if there's already a year
						m_selection.Set(GetYearStartPosition(), GetYearStartPosition() + 4);

					m_selection.Replace(String.Format("{0,4:0000}", value));	// set the year
					
					AdjustMaxMonthAndDay();	// adjust the month and/or day if they're out of range
				}
			}
		}

		/// <summary>
		///   Gets or sets the month, day, and year on the textbox using a <see cref="DateTime" /> object. </summary>
		/// <remarks>
		///   This property gets and sets the <see cref="DateTime" /> boxed inside an <c>object</c>.
		///   This makes it flexible, so that if the textbox does not hold a valid date, a <c>null</c> is returned, 
		///   instead of having to worry about an exception being thrown. </remarks>
		/// <example><code>
		///   object obj = txtDate.Behavior.Value;
		///   
		///   if (obj != null)
		///   {
		///     DateTime dtm = (DateTime)obj;
		///     ...
		///   } </code></example>
		/// <seealso cref="Month" />
		/// <seealso cref="Day" />
		/// <seealso cref="Year" />
		public object Value
		{
			get 
			{
				try
				{
					return new DateTime(Year, Month, Day); 
				}
				catch
				{
					return null;
				}
			}
			set
			{
				DateTime dt = (DateTime)value;
				m_textBox.Text = GetFormattedDate(dt.Year, dt.Month, dt.Day);
			}
		}

		/// <summary>
		///   Sets the month, day, and year on the textbox. </summary>
		/// <param name="year">
		///   The year to set. </param>
		/// <param name="month">
		///   The month to set. </param>
		/// <param name="day">
		///   The day to set. </param>
		/// <remarks>
		///   This is a convenience method to set each value individually using a single method. 
		///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. </remarks>
		public void SetDate(int year, int month, int day)
		{
			Value = new DateTime(year, month, day);
		}
		
		/// <summary>
		///   Checks if the textbox's date is valid and falls within the allowed range. </summary>
		/// <returns>
		///   If the value is valid and falls within the allowed range, the return value is true; otherwise it is false. </returns>
		/// <seealso cref="RangeMin" />
		/// <seealso cref="RangeMax" />
		public override bool IsValid()
		{
			try
			{
				return IsWithinRange(new DateTime(Year, Month, Day));
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		///   Gets the error message used to notify the user to enter a valid date value 
		///   within the allowed range. </summary>
		/// <seealso cref="IsValid" />
		public override string ErrorMessage
		{
			get
			{
				return "Please specify a date between " + GetFormattedDate(m_rangeMin.Year, m_rangeMin.Month, m_rangeMin.Day) + " and " + GetFormattedDate(m_rangeMax.Year, m_rangeMax.Month, m_rangeMax.Day) + ".";
			}
		}

		/// <summary>
		///   Gets or sets the minimum value allowed. </summary>
		/// <remarks>
		///   By default, this property is set to DateTime(1900, 1, 1).
		///   The range is actively checked as the user enters the date and 
		///   when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
		/// <seealso cref="RangeMax" />
		public DateTime RangeMin
		{
			get 
			{ 
				return m_rangeMin; 
			}
			set 
			{ 
				if (value < new DateTime(1900, 1, 1))
					throw new ArgumentOutOfRangeException("RangeMin", value, "Minimum value may not be older than January 1, 1900");
				
				m_rangeMin = value;
				UpdateText();
			}
		}

		/// <summary>
		///   Gets or sets the maximum value allowed. </summary>
		/// <remarks>
		///   By default, this property is set to <see cref="DateTime.MaxValue" />.
		///   The range is actively checked as the user enters the date and 
		///   when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
		/// <seealso cref="RangeMin" />
		public DateTime RangeMax
		{
			get 
			{ 
				return m_rangeMax; 
			}
			set 
			{ 
				m_rangeMax = value; 
				UpdateText();
			}
		}

		/// <summary>
		///   Checks if a date value falls within the allowed range. </summary>
		/// <param name="dt">
		///   The date value to check. </param>
		/// <returns>
		///   If the value is within the allowed range, the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   Only the date portion is checked; the time is ignored. </remarks>
		/// <seealso cref="RangeMin" />
		/// <seealso cref="RangeMax" />
		/// <seealso cref="IsValid" />
		public bool IsWithinRange(DateTime dt)
		{
			DateTime date = new DateTime(dt.Year, dt.Month, dt.Day);
			return (date >= m_rangeMin && date <= m_rangeMax);
		}

		/// <summary>
		///   Gets or sets the character used to separate the month, day, and year values of the date. </summary>
		/// <remarks>
		///   By default, this property is set according to the user's system. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		public char Separator
		{
			get 
			{ 
				return m_separator; 
			}
			set
			{
				if (m_separator == value)
					return;

				Debug.Assert(value != 0);
				Debug.Assert(!Char.IsDigit(value));

				m_separator = value;
				UpdateText();
			}
		}

		/// <summary>
		///   Gets or sets whether the day should be shown before the month or after it. </summary>
		/// <remarks>
		///   By default, this property is set according to the user's system. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="Flag.DayBeforeMonth" />
		public bool ShowDayBeforeMonth
		{
			get 
			{ 
				return HasFlag((int)Flag.DayBeforeMonth); 
			}
			set 
			{ 
				ModifyFlags((int)Flag.DayBeforeMonth, value);	
			}
		}

		/// <summary>
		///   Retrieves the textbox's text in valid form. </summary>
		/// <returns>
		///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
		protected override string GetValidText()
		{
			string text = m_textBox.Text; 

			if (text == "")
				return text;

			if (IsValid())
				return GetFormattedDate(Year, Month, Day);

			// If the date is empty, try using today
			if (Year == 0 && Month == 0 && Day == 0)
				Value = DateTime.Today;

			int year = GetValidYear();
			int month = GetValidMonth();
			int day = GetValidDay();

			if (!IsWithinRange(new DateTime(year, month, day)))
				month = GetMinMonth();

			if (!IsWithinRange(new DateTime(year, month, day)))
				day = GetMaxDay();

			return GetFormattedDate(year, month, day);
		}

		/// <summary>
		///   Retrieves the textbox's text in valid form. </summary>
		/// <returns>
		///   This is just an <c>internal</c> version of <see cref="GetValidText" /> designed to be 
		///   accessed by the <see cref="DateTimeBehavior" /> class which needs it </returns>
		internal string GetValidTextForDateTime()
		{
			return GetValidText();
		}

		/// <summary>
		///   Formats a year, month, and day value into a string based on the proper format (mm/dd/yyyy or dd/mm/yyyy). </summary>
		/// <param name="year">
		///   The year value. </param>
		/// <param name="month">
		///   The month value. </param>
		/// <param name="day">
		///   The day value. </param>
		/// <returns>
		///   The return value is the formatted date value. </returns>
		public string GetFormattedDate(int year, int month, int day)
		{
			if (ShowDayBeforeMonth)
				return String.Format("{0,2:00}{1}{2,2:00}{3}{4,4:0000}", day, m_separator, month, m_separator, year);
			return String.Format("{0,2:00}{1}{2,2:00}{3}{4,4:0000}", month, m_separator, day, m_separator, year);
		}
	}

	
	/////////////////////////////////////////////////////////////////////////////
	// Time behavior

	/// <summary>
	///   Behavior class which handles input of time values. </summary>
	/// <remarks>
	///   This behavior supports time values in 12 or 24 hour format, with or without seconds. 
	///   It is designed to let the user enter a time value quickly and accurately.  
	///   As the user enters the digits, the colons are automatically filled in. The user may only remove 
	///   characters from the right side of the value entered. This helps to keep the value properly formatted. 
	///   The user may also use the up/down arrow keys to increment/decrement the hour, minute, second, or AM/PM, 
	///   depending on the location of the caret.  </remarks>
	/// <seealso cref="DateBehavior" />
	/// <seealso cref="DateTimeBehavior" />
	public class TimeBehavior : Behavior
	{
		// Fields
		private DateTime m_rangeMin = new DateTime(1900, 1, 1,  0,  0,  0);
		private DateTime m_rangeMax = new DateTime(1900, 1, 1, 23, 59, 59);
		private char m_separator = ':';
		private string m_am = "AM";
		private string m_pm = "PM";
		private int m_ampmLength = 2;
		
		/// <summary>
		///   The starting zero-based position of the hour on the texbox. </summary>
		/// <remarks>
		///   This is 0 by default, however it may be changed to allow 
		///   another value to be placed in front of the time, such as a date. </remarks>
		protected int m_hourStart = 0;

		/// <summary>
		///   Internal values that are added/removed to the <see cref="Behavior.Flags" /> property by other
		///   properties of this class. </summary>
		[Flags]
		protected enum Flag
		{
			/// <summary> The hour is shown in 24-hour format (00 to 23). </summary>
			/// <seealso cref="Show24HourFormat" />
			TwentyFourHourFormat						= 0x00020000,

			/// <summary> The seconds are also shown. </summary>
			/// <seealso cref="ShowSeconds" />
			WithSeconds									= 0x00040000,
		};

		/// <summary>
		///   Initializes a new instance of the TimeBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor determines the <see cref="Separator" /> and time format from the user's system. </remarks>
		/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
		public TimeBehavior(TextBoxBase textBox) :
			base(textBox, true)
		{
			// Get the system's time separator
			m_separator = DateTimeFormatInfo.CurrentInfo.TimeSeparator[0];

			// Determine if it's in 24-hour format
			string shortTime = DateTimeFormatInfo.CurrentInfo.ShortTimePattern;
			if (shortTime.IndexOf('H') >= 0)
				m_flags |= (int)Flag.TwentyFourHourFormat;			
			
			// Get the AM and PM symbols
			m_am = DateTimeFormatInfo.CurrentInfo.AMDesignator;
			m_pm = DateTimeFormatInfo.CurrentInfo.PMDesignator;
			m_ampmLength = m_am.Length;
			
			// Verify the lengths are the same; otherwise use the default
			if (m_ampmLength == 0 || m_ampmLength != m_pm.Length)
			{
				m_am = "AM";
				m_pm = "PM";
				m_ampmLength = 2;
			}				
		}

		/// <summary>
		///   Initializes a new instance of the TimeBehavior class by copying it from 
		///   another TimeBehavior object. </summary>
		/// <param name="behavior">
		///   The TimeBehavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		public TimeBehavior(TimeBehavior behavior) :
			base(behavior)
		{
			m_rangeMin = behavior.m_rangeMin;
			m_rangeMax = behavior.m_rangeMax;
			m_separator = behavior.m_separator;
			m_am = behavior.m_am;
			m_pm = behavior.m_pm;
			m_ampmLength = behavior.m_ampmLength;
			m_hourStart = behavior.m_hourStart;
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyDown event. </remarks>
		/// <seealso cref="Control.KeyDown" />
		protected override void HandleKeyDown(object sender, KeyEventArgs e)
		{
			TraceLine("TimeBehavior.HandleKeyDown " + e.KeyCode);	

			// Check to see if it's read only
			if (m_textBox.ReadOnly)
				return;
								
			e.Handled = true;
			
			switch (e.KeyCode)
			{
				case Keys.Delete:
				{
					// If deleting make sure it's the last character or that
					// the selection goes all the way to the end of the text

					int start, end;
					m_selection.Get(out start, out end);

					string text = m_textBox.Text;
					int length = text.Length;

					if (end != length)
					{
						if (!(end == start && end == length - 1))
							return;
					}

					m_noTextChanged = true;
					break;
				}

				case Keys.Up:
				{
					// If pressing the UP arrow, increment the corresponding value.
					
					int start, end;
					m_selection.Get(out start, out end);
					
					if (start >= GetHourStartPosition() && start <= GetHourStartPosition() + 2)
					{
						int hour = Hour;
						if (hour >= GetMinHour(false))
						{
							// Handle moving up through the noon hour
							string ampm = AMPM;
							if (IsValidAMPM(ampm))
							{
								if (hour == 11)
								{
									if (ampm == m_pm)  // stop at midnight
										return;
									SetAMPM(false); 
								}
								else if (hour == 12)
									hour = 0;
							}

							if (hour < GetMaxHour(false))
								Hour = ++hour;
						}
					}			
					else if (start >= GetMinuteStartPosition() && start <= GetMinuteStartPosition() + 2)
					{
						int minute = Minute;
						if (minute >= GetMinMinute() && minute < GetMaxMinute())
							Minute = ++minute;
					}
					else if (start >= GetAMPMStartPosition() && start <= GetAMPMStartPosition() + m_ampmLength)
					{
						string ampm = AMPM;
						SetAMPM(!IsValidAMPM(ampm) || ampm == m_pm);
					}
					else if (start >= GetSecondStartPosition() && start <= GetSecondStartPosition() + 2)
					{
						int second = Second;
						if (second >= GetMinSecond() && second < GetMaxSecond())
							Second = ++second;
					}

					return;
				}

				case Keys.Down:
				{
					// If pressing the DOWN arrow, decrement the corresponding value.
					
					int start, end;
					m_selection.Get(out start, out end);
					
					if (start >= GetHourStartPosition() && start <= GetHourStartPosition() + 2)
					{
						int hour = Hour;
						if (hour <= GetMaxHour(false))
						{
							// Handle moving up through the noon hour
							string ampm = AMPM;
							if (IsValidAMPM(ampm))
							{
								if (hour == 12)
								{
									if (ampm == m_am)	// stop at midnight
										return;
									SetAMPM(true);
								}
								else if (hour == 1)
									hour = 13;
							}

							if (hour > GetMinHour(false))
								Hour = --hour;
						}
					}			
					else if (start >= GetMinuteStartPosition() && start <= GetMinuteStartPosition() + 2)
					{
						int minute = Minute;
						if (minute > GetMinMinute() && minute <= GetMaxMinute())
							Minute = --minute;
					}
					else if (start >= GetAMPMStartPosition() && start <= GetAMPMStartPosition() + m_ampmLength)
					{
						string ampm = AMPM;
						SetAMPM(!IsValidAMPM(ampm) || ampm == m_pm);
					}
					else if (start >= GetSecondStartPosition() && start <= GetSecondStartPosition() + 2)
					{
						int second = Second;
						if (second > GetMinSecond() && second <= GetMaxSecond())
							Second = --second;
					}
					return;
				}
			}

			base.HandleKeyDown(sender, e);
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyPress event. </remarks>
		/// <seealso cref="Control.KeyPress" />
		protected override void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			TraceLine("TimeBehavior.HandleKeyPress " + e.KeyChar);	
			
			// Check to see if it's read only
			if (m_textBox.ReadOnly)
				return;
					
			char c = e.KeyChar;
			e.Handled = true;
			m_noTextChanged = true;
		
			int start, end;
			m_selection.Get(out start, out end);

			string text = m_textBox.Text;
			int length = text.Length;

			// Check for a non-printable character (such as Ctrl+C)
			if (Char.IsControl(c))
			{
				if (c == (short)Keys.Back && start != length)
				{
					SendKeys.Send("{LEFT}");  // move the cursor left
					return;
				}
				
				// Allow backspace only if the cursor is all the way to the right
				base.HandleKeyPress(sender, e);
				return;
			}
			
			// Add the digit depending on its location
			if (start == m_hourStart)		// FIRST DIGIT
			{
				if (IsValidHourDigit(c, 0))
				{
					if (length > start)
					{
						m_selection.SetAndReplace(start, start + 1, c.ToString());
						
						if (length > start + 1)
						{
							// If the second digit is no longer valid, correct and select it
							if (!IsValidHour(Hour, false))
							{
								m_selection.SetAndReplace(start + 1, start + 2, GetMinHourDigit(1).ToString());
								m_selection.Set(start + 1, start + 2);
							}
						}
					}
					else
						base.HandleKeyPress(sender, e);
				}
				else if (length == start && IsValidHourDigit(c, 1))
					m_selection.SetAndReplace(start, start + 2, "0" + c);					
				else
					ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}

			else if (start == m_hourStart + 1)	// SECOND DIGIT
			{
				if (IsValidHourDigit(c, 1))
				{
					if (length > start)
						m_selection.SetAndReplace(start, start + 1, c.ToString());					
					else
						base.HandleKeyPress(sender, e);
				}
				else if (c == m_separator && length == start && IsValidHour(ToInt("0" + text[m_hourStart]), false))
					m_selection.SetAndReplace(m_hourStart, start, "0" + text[m_hourStart] + c);					
				else
					ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}
			
			else if (start == m_hourStart + 2)	// FIRST COLON
			{
				int colon = 0;
				if (c == m_separator)
					colon = 1;
				else
					colon = (IsValidMinuteDigit(c, 0) ? 2 : 0);
				
				// If we need the colon, enter it
				if (colon != 0)
					m_selection.SetAndReplace(start, start + 1, m_separator.ToString());
				
				// If the colon is to be preceded by a valid digit, "type" it in.
				if (colon == 2)
					SendKeys.Send(c.ToString());
				else
					ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}
					
			else if (start == m_hourStart + 3)	// THIRD DIGIT
			{
				if (IsValidMinuteDigit(c, 0))
				{
					if (length > start)
					{
						m_selection.SetAndReplace(start, start + 1, c.ToString());
						
						if (length > start + 1)
						{
							if (!IsValidMinute(Minute))
							{
								m_selection.SetAndReplace(start + 1, start + 2, GetMinMinuteDigit(1).ToString());
								m_selection.Set(start + 1, start + 2);
							}
						}
					}
					else
						base.HandleKeyPress(sender, e);
				}
				else
					ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}
			
			else if (start == m_hourStart + 4)	// FOURTH DIGIT
			{
				if (IsValidMinuteDigit(c, 1))
				{
					if (length > start)
						m_selection.SetAndReplace(start, start + 1, c.ToString());
					else
						base.HandleKeyPress(sender, e);
					
					// Show the AM/PM symbol if we're not showing seconds
					if (!ShowSeconds)
						ShowAMPM();
				}				
				else
					ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}
					
			else if (start == m_hourStart + 5)	// SECOND COLON	OR FIRST SPACE (seconds' first digit or AM/PM)
			{
				if (ShowSeconds)
				{
					int colon = 0;
					if (c == m_separator)
						colon = 1;
					else
						colon = (IsValidSecondDigit(c, 0) ? 2 : 0);
					
					// If we need the slash, enter it
					if (colon != 0)
					{					
						int replace = (start < length && text[start] != ' ') ? 1 : 0;
						m_selection.SetAndReplace(start, start + replace, m_separator.ToString());
					}
					
					// If the colon is to be preceded by a valid digit, "type" it in.
					if (colon == 2)
						SendKeys.Send(c.ToString());
				}
				else if (!Show24HourFormat)
				{
					if (c == ' ')
						m_selection.SetAndReplace(start, start + 1, c.ToString());
					ShowAMPM();
				}

				ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}

			else if (start == m_hourStart + 6)	// FIFTH DIGIT - first digit of seconds or AM/PM
			{
				if (ShowSeconds)
				{
					if (IsValidSecondDigit(c, 0))
					{
						if (length > start)
						{
							int replace = (start < length && text[start] != ' ') ? 1 : 0;
							m_selection.SetAndReplace(start, start + replace, c.ToString());
						}
						else
							base.HandleKeyPress(sender, e);
					}
				}
				
				ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}

			else if (start == m_hourStart + 7)	// SIXTH DIGIT - second digit of seconds or AM/PM
			{
				if (ShowSeconds)
				{
					if (IsValidSecondDigit(c, 1))
					{
						if (length > start)
						{
							int replace = (start < length && text[start] != ' ') ? 1 : 0;
							m_selection.SetAndReplace(start, start + replace, c.ToString());
						}
						else
							base.HandleKeyPress(sender, e);

						// Show the AM/PM symbol if we're not in 24-hour format
						ShowAMPM();
					}
				}

				ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}
				
			else if (start == m_hourStart + 8)	// FIRST SPACE (with seconds showing)
			{
				if (ShowSeconds && !Show24HourFormat)
				{
					if (c == ' ')
					{
						m_selection.SetAndReplace(start, start + 1, c.ToString());
						ShowAMPM();
					}
				} 

				ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}

			else 		// AM/PM
				ChangeAMPM(c);
		}

		/// <summary>
		///   Converts an integer value to a 2-digit string (00 - 99). </summary>
		/// <param name="value">
		///   The number to convert. </param>
		/// <returns>
		///   The return value is the formatted string. </returns>
		/// <remarks>
		///   This is convenience method for formatting 2 digit
		///   values such as the hour and minute. </remarks>
		protected static string TwoDigits(int value)
		{
			return String.Format("{0,2:00}", value);
		}

		/// <summary>
		///   Retrieves the zero-based position of the hour inside the texbox. </summary>
		/// <returns>
		///   The return value is the starting position of the hour. </returns>
		protected int GetHourStartPosition()
		{
			return m_hourStart;
		}

		/// <summary>
		///   Retrieves the zero-based position of the minute inside the texbox. </summary>
		/// <returns>
		///   The return value is the starting position of the minute. </returns>
		protected int GetMinuteStartPosition()
		{
			return m_hourStart + 3;
		}

		/// <summary>
		///   Retrieves the zero-based position of the second inside the texbox. </summary>
		/// <returns>
		///   The return value is the starting position of the second. </returns>
		protected int GetSecondStartPosition()
		{
			return m_hourStart + 6;
		}

		/// <summary>
		///   Retrieves the zero-based position of the AM/PM inside the texbox. </summary>
		/// <returns>
		///   The return value is the starting position of the AM/PM. </returns>
		/// <remarks>
		///   This is based on whether the seconds are being shown or not. </remarks>
		protected int GetAMPMStartPosition()
		{
			return m_hourStart + (ShowSeconds ? 9 : 6);
		}

		/// <summary>
		///   Retrieves the maximum value for the hour. </summary>
		/// <param name="force24HourFormat">
		///   If true, the maximum is 23, regardless of the <see cref="Show24HourFormat" /> property; 
		///   otherwise it is based on the <see cref="Show24HourFormat" /> property. </param>
		/// <returns>
		///   The return value is the maximum value for the hour (23 or 12). </returns>
		/// <remarks>
		///   Note: This value is not based on <see cref="RangeMax" />. </remarks>
		protected int GetMaxHour(bool force24HourFormat)
		{
			return (force24HourFormat || Show24HourFormat) ? 23 : 12;
		}

		/// <summary>
		///   Retrieves the minimum value for the hour. </summary>
		/// <param name="force24HourFormat">
		///   If true, the minimum is 0, regardless of the <see cref="Show24HourFormat" /> property; 
		///   otherwise it is based on the <see cref="Show24HourFormat" /> property. </param>
		/// <returns>
		///   The return value is the minimum value for the hour (0 or 1). </returns>
		/// <remarks>
		///   Note: This value is not based on <see cref="RangeMin" />. </remarks>
		protected int GetMinHour(bool force24HourFormat)
		{
			return (force24HourFormat || Show24HourFormat) ? 0 : 1;
		}

		/// <summary>
		///   Retrieves the maximum value for the minute: 59. </summary>
		/// <returns>
		///   The return value is always 59. </returns>
		/// <remarks>
		///   Note: This value is not based on <see cref="RangeMax" />. </remarks>
		protected int GetMaxMinute()
		{
			return 59;
		}

		/// <summary>
		///   Retrieves the minimum value for the minute: 0. </summary>
		/// <returns>
		///   The return value is always 0. </returns>
		/// <remarks>
		///   Note: This value is not based on <see cref="RangeMin" />. </remarks>
		protected int GetMinMinute()
		{
			return 0;
		}

		/// <summary>
		///   Retrieves the maximum value for the second: 59. </summary>
		/// <returns>
		///   The return value is always 59. </returns>
		/// <remarks>
		///   Note: This value is not based on <see cref="RangeMax" />. </remarks>
		protected int GetMaxSecond()
		{
			return 59;
		}

		/// <summary>
		///   Retrieves the minimum value for the second: 0. </summary>
		/// <returns>
		///   The return value is always 0. </returns>
		/// <remarks>
		///   Note: This value is not based on <see cref="RangeMin" />. </remarks>
		protected int GetMinSecond()
		{
			return 0;
		}

		/// <summary>
		///   Retrieves the maximum digit that an hour value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the hour (0 or 1). </param>
		/// <returns>
		///   The return value is the maximum digit that it can be. </returns>
		protected char GetMaxHourDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);

			// First digit
			if (position == 0)
				return Show24HourFormat ? '2' : '1';

			// Second digit
			string text = m_textBox.Text;
			char firstDigit = (text.Length > GetHourStartPosition()) ? text[GetHourStartPosition()] : '0';
			Debug.Assert(firstDigit != 0);  // must have a valid first digit at this point

			// Use the first digit to determine the second digit's max
			if (firstDigit == '2')
				return '3';
			if (firstDigit == '1' && !Show24HourFormat)
				return '2';
			return '9';
		}

		/// <summary>
		///   Retrieves the minimum digit that an hour value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the hour (0 or 1). </param>
		/// <returns>
		///   The return value is the minimum digit that it can be. </returns>
		protected char GetMinHourDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);

			// First digit
			if (position == 0)
				return '0';

			// Second digit
			string text = m_textBox.Text;
			char firstDigit = (text.Length > GetHourStartPosition()) ? text[GetHourStartPosition()] : '0';
			Debug.Assert(firstDigit != 0);  // must have a valid first digit at this point

			// If the first digit is a 0 and we're not in 24-hour format, don't allow 0
			if (firstDigit == '0' && !Show24HourFormat)
				return '1';

			// For all other cases it's always 0
			return '0';
		}

		/// <summary>
		///   Checks if a digit is valid for the hour at one of its two character positions. </summary>
		/// <param name="c">
		///   The digit to check. </param>
		/// <param name="position">
		///   The position of the digit of the hour (0 or 1). </param>
		/// <returns>
		///   If the digit is valid for the hour (at the given position), the return value is true; otherwise it is false. </returns>
		protected bool IsValidHourDigit(char c, int position)
		{
			return (c >= GetMinHourDigit(position) && c <= GetMaxHourDigit(position));
		}

		/// <summary>
		///   Checks if a value represents a valid hour. </summary>
		/// <param name="hour">
		///   The value to check. </param>
		/// <param name="force24HourFormat">
		///   If true, the range is based on a 24-hour format, regardless of the <see cref="Show24HourFormat" /> property; 
		///   otherwise it is based on the <see cref="Show24HourFormat" /> property. </param>
		/// <returns>
		///   If the value is a valid hour, the return value is true; otherwise it is false. </returns>
		protected bool IsValidHour(int hour, bool force24HourFormat)
		{
			return (hour >= GetMinHour(force24HourFormat) && hour <= GetMaxHour(force24HourFormat));
		}

		/// <summary>
		///   Retrieves the maximum digit that a minute value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the minute (0 or 1). </param>
		/// <returns>
		///   The return value is the maximum digit that it can be. </returns>
		protected char GetMaxMinuteDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);
			return (position == 0 ? '5' : '9');
		}

		/// <summary>
		///   Retrieves the minimum digit that a minute value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the minute (0 or 1). </param>
		/// <returns>
		///   The return value is the minimum digit that it can be. </returns>
		protected char GetMinMinuteDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);	
			return '0';
		}

		/// <summary>
		///   Checks if a digit is valid for the minute at one of its two character positions. </summary>
		/// <param name="c">
		///   The digit to check. </param>
		/// <param name="position">
		///   The position of the digit of the minute (0 or 1). </param>
		/// <returns>
		///   If the digit is valid for the minute (at the given position), the return value is true; otherwise it is false. </returns>
		protected bool IsValidMinuteDigit(char c, int position)
		{
			return (c >= GetMinMinuteDigit(position) && c <= GetMaxMinuteDigit(position));
		}

		/// <summary>
		///   Checks if a value represents a valid minute. </summary>
		/// <param name="minute">
		///   The value to check. </param>
		/// <returns>
		///   If the value is a valid minute, the return value is true; otherwise it is false. </returns>
		protected bool IsValidMinute(int minute)
		{
			return (minute >= GetMinMinute() && minute <= GetMaxMinute());
		}

		/// <summary>
		///   Retrieves the maximum digit that a "second" value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the second (0 or 1). </param>
		/// <returns>
		///   The return value is the maximum digit that it can be. </returns>
		protected char GetMaxSecondDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);
			return (position == 0 ? '5' : '9');
		}

		/// <summary>
		///   Retrieves the minimum digit that a "second" value can take, at one of its two character positions. </summary>
		/// <param name="position">
		///   The position of the digit of the second (0 or 1). </param>
		/// <returns>
		///   The return value is the minimum digit that it can be. </returns>
		protected char GetMinSecondDigit(int position)
		{
			Debug.Assert(position >= 0 && position <= 1);	
			return '0';
		}

		/// <summary>
		///   Checks if a digit is valid for the "second" at one of its two character positions. </summary>
		/// <param name="c">
		///   The digit to check. </param>
		/// <param name="position">
		///   The position of the digit of the second (0 or 1). </param>
		/// <returns>
		///   If the digit is valid for the second (at the given position), the return value is true; otherwise it is false. </returns>
		protected bool IsValidSecondDigit(char c, int position)
		{
			return (c >= GetMinSecondDigit(position) && c <= GetMaxSecondDigit(position));
		}

		/// <summary>
		///   Checks if a value represents a valid second. </summary>
		/// <param name="second">
		///   The value to check. </param>
		/// <returns>
		///   If the value is a valid second, the return value is true; otherwise it is false. </returns>
		protected bool IsValidSecond(int second)
		{
			return (second >= GetMinSecond() && second <= GetMaxSecond());
		}

		/// <summary>
		///   Shows the AM symbol if not in 24-hour format and it's not already shown. </summary>
		protected void ShowAMPM()
		{
			if (!Show24HourFormat && !IsValidAMPM(AMPM))
				SetAMPM(true);
		}

		/// <summary>
		///   Sets the AM or PM symbol if not in 24-hour format. </summary>
		/// <param name="am">
		///   If true, sets the AM symbol; otherwise it sets the PM symbol. </param>
		/// <seealso cref="AMPM" />
		public void SetAMPM(bool am)
		{
			if (Show24HourFormat)
				return;

			using (Selection.Saver savedSelection = new Selection.Saver(m_textBox))	// remember the current selection
			{
				m_selection.Set(GetAMPMStartPosition() - 1, GetAMPMStartPosition() + m_ampmLength);					
				m_selection.Replace(" " + (am ? m_am : m_pm));	// set the AM/PM
			}
		}

		/// <summary>
		///   Changes the AM/PM symbol based on a character entered by the user. </summary>
		/// <param name="c">
		///   The character entered by the user, such as 'a' or 'p'. </param>
		/// <returns>
		///   If the AM/PM symbol is changed, the return value is true; otherwise it is false. </returns>
		protected bool ChangeAMPM(char c)
		{
			if (Show24HourFormat)
				return false;

			string text = m_textBox.Text;
			int length = text.Length;

			int position = GetAMPMPosition(text);
			if (position == 0)
				return false;

			int start, end;
			m_selection.Get(out start, out end);

			char cUpper = Char.ToUpper(c);

			switch (cUpper)
			{
				case 'A':
				case 'P':
					SetAMPM(cUpper == 'A');

					if (cUpper == Char.ToUpper(m_am[0]) || cUpper == Char.ToUpper(m_pm[0]))
					{
						// Move the cursor right, if we're in front of the AM/PM symbols
						if (start == position)
							SendKeys.Send("{RIGHT}");

						// Move the cursor right twice, if we're in front of the space in front of the AM/PM symbols
						if (start + 1 == position)
						{
							SendKeys.Send("{RIGHT}");
							SendKeys.Send("{RIGHT}");
						}
					}
					return true;

				default:
					// Handle entries after the first character of the AM/PM symbol -- allow the user to enter each character
					if (start > position)
					{
						// Check if we're adding a character of the AM/PM symbol (after the first one)
						if ((length == start && !IsValidAMPM(AMPM)) || (length == end && end != start))
						{
							string ampmToUse = Char.ToUpper(text[position]) == Char.ToUpper(m_am[0]) ? m_am : m_pm;
							if (cUpper == Char.ToUpper(ampmToUse[start - position]))
							{
								m_selection.Replace(ampmToUse.Substring(start - position));	// set the rest of the AM/PM
								m_selection.Set(start, start);  // Reset the selection so that the cursor can be moved
								return ChangeAMPM(c); // move the cursor (below)
							}
						}

						// Check if the AM/PM symbol is OK and we just need to move over one
						if (length > start && end == start && cUpper == Char.ToUpper(text[start]))
						{
							SendKeys.Send("{RIGHT}");
							return true;
						}
					}
					break;
			}

			return false;
		}

		/// <summary>
		///   Retrieves the zero-based position of the AM/PM symbol on a given text. </summary>
		/// <param name="text">
		///   The text to parse and find the position of the AM/PM symbol. </param>
		/// <returns>
		///   The return value is the zero-based position of the AM/PM symbol. </returns>
		private int GetAMPMPosition(string text)
		{
			int position = text.IndexOf(' ' + m_am);
			return ((position < 0) ? text.IndexOf(' ' + m_pm) : position) + 1;
		}

		/// <summary>
		///   Checks if a string is a valid AM or PM symbol. </summary>
		/// <param name="ampm">
		///   The value to check. </param>
		/// <returns>
		///   If the value is a valid AM or PM symbol, the return value is true; otherwise it is false. </returns>
		protected bool IsValidAMPM(string ampm)
		{
			return (ampm == m_am || ampm == m_pm);
		}

		/// <summary>
		///   Retrieves the hour on the textbox as a valid value. </summary>
		/// <param name="force24HourFormat">
		///   If true, the value is validated based on a 24-hour format, regardless of the <see cref="Show24HourFormat" /> property; 
		///   otherwise it is based on the <see cref="Show24HourFormat" /> property. </param>
		/// <returns>
		///   The return value is the valid value for the hour. </returns>
		/// <remarks>
		///   The method checks the value of the hour on the textbox.  
		///   If it is a valid hour, it returns it.  
		///   If it is less than it should be, the minimum is returned.
		///   If it is more than it should be, the maximum is returned. </remarks>
		protected int GetValidHour(bool force24HourFormat)
		{
			int hour = Hour;
			
			// It it's outside the range, fix it
			if (hour < GetMinHour(force24HourFormat))
				hour = GetMinHour(force24HourFormat);
			else if (hour > GetMaxHour(force24HourFormat))
				hour = GetMaxHour(force24HourFormat);
			
			return hour;
		}

		/// <summary>
		///   Retrieves the minute on the textbox as a valid value. </summary>
		/// <returns>
		///   The return value is the valid value for the minute. </returns>
		/// <remarks>
		///   The method checks the value of the minute on the textbox.  
		///   If it is a valid minute, it returns it.  
		///   If it is less than it should be, the minimum is returned.
		///   If it is more than it should be, the maximum is returned. </remarks>
		protected int GetValidMinute()
		{
			int minute = Minute;
			
			// It it's outside the range, fix it
			if (minute < GetMinMinute())
				minute = GetMinMinute();
			else if (minute > GetMaxMinute())
				minute = GetMaxMinute();
			
			return minute;
		}

		/// <summary>
		///   Retrieves the second on the textbox as a valid value. </summary>
		/// <returns>
		///   The return value is the valid value for the second. </returns>
		/// <remarks>
		///   The method checks the value of the second on the textbox.  
		///   If it is a valid second, it returns it.  
		///   If it is less than it should be, the minimum is returned.
		///   If it is more than it should be, the maximum is returned. </remarks>
		protected int GetValidSecond()
		{
			int second = Second;
			if (second < GetMinSecond())
				second = GetMinSecond();
			else if (second > GetMaxSecond())
				second = GetMaxSecond();
			
			return second;
		}

		/// <summary>
		///   Retrieves the AM/PM symbol on the textbox as a valid value. </summary>
		/// <returns>
		///   The return value is the valid value for the AM/PM symbol. </returns>
		/// <remarks>
		///   The method checks the value of the AM/PM symbol on the textbox.  
		///   If it is a valid AM/PM symbol, it returns it; otherwise it returns the AM symbol. </remarks>
		protected string GetValidAMPM()
		{
			string ampm = AMPM;
			if (!IsValidAMPM(ampm))
				return m_am;			
			
			return ampm;
		}

		/// <summary>
		///   Gets or sets the hour on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid hour. </exception>
		/// <remarks>
		///   If the hour is not valid on the textbox, this property will return -1.
		///   This property must be set with a valid hour -- between 0 and 23. </remarks>
		/// <seealso cref="Minute" />
		/// <seealso cref="Second" />
		public int Hour
		{
			get
			{			
				string text = m_textBox.Text;

				int startPos = GetHourStartPosition();

				// If there's already a separator, extract the value in front of it
				int sepPos = text.IndexOf(m_separator);
				if (sepPos > 0)
				{
					startPos = sepPos - 2;
					if (startPos < 0)
						startPos = 0;
				}

				if (text.Length >= startPos + 1)
					return ToInt(text.Substring(startPos, 2).Trim());

				return -1;
			}			
			set 
			{
				if (!IsValidHour(value, false))
					throw new ArgumentOutOfRangeException();
				
				using (Selection.Saver savedSelection = new Selection.Saver(m_textBox))	// remember the current selection
				{					
					if (Hour >= 0)		// see if there's already an hour
						m_selection.Set(GetHourStartPosition(), GetHourStartPosition() + 3);
					
					// Convert it to AM/PM hour if necessary
					string ampm = "";
					if (!Show24HourFormat && value > 12)
						value = ConvertToAMPMHour(value, out ampm);

					m_selection.Replace(TwoDigits(value) + m_separator);	// set the hour

					// Change the AM/PM if it's present
					if (ampm != "" && IsValidAMPM(AMPM))
						SetAMPM(ampm == m_am);
				}					
			}
		}

		/// <summary>
		///   Gets or sets the minute on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid minute. </exception>
		/// <remarks>
		///   If the minute is not valid on the textbox, this property will return -1.
		///   This property must be set with a valid minute -- between 0 and 59. </remarks>
		/// <seealso cref="Hour" />
		/// <seealso cref="Second" />
		public int Minute
		{
			get
			{
				string text = m_textBox.Text;				
				int startPos = text.IndexOf(m_separator, m_hourStart) + 1;

				if (startPos > 0 && text.Length >= startPos + 2)
					return ToInt(text.Substring(startPos, 2));
				
				return -1;				
			}			
			set
			{
				if (!IsValidMinute(value))
					throw new ArgumentOutOfRangeException();						
				
				using (Selection.Saver savedSelection = new Selection.Saver(m_textBox))	// remember the current selection
				{					
					if (Minute >= 0)		// see if there's already a minute
						m_selection.Set(GetMinuteStartPosition(), GetMinuteStartPosition() + 2 + (ShowSeconds ? 1 : 0));
					
					string text = TwoDigits(value);
					if (ShowSeconds)
						text += m_separator;
					
					m_selection.Replace(text);	// set the minute

					// Append the AM/PM if no seconds come after and it's not in 24-hour format
					if (!ShowSeconds)
						ShowAMPM();
				}
			}
		}

		/// <summary>
		///   Gets or sets the second on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid second. </exception>
		/// <remarks>
		///   If the second is not valid on the textbox, this property will return -1.
		///   This property must be set with a valid second -- between 0 and 59. </remarks>
		/// <seealso cref="Hour" />
		/// <seealso cref="Minute" />
		public int Second
		{
			get
			{
				string text = m_textBox.Text;
				int startPos = text.IndexOf(m_separator, m_hourStart);
				if (startPos > 0)
				{
					startPos = text.IndexOf(m_separator, startPos + 1) + 1;
					if (startPos == 0)
						return -1;
				}

				if (text.Length >= startPos + 2 && Char.IsDigit(text[startPos]) && Char.IsDigit(text[startPos + 1]))
					return ToInt(text.Substring(startPos, 2));

				return -1;
			}			
			set
			{
				if (!IsValidSecond(value))
					throw new ArgumentOutOfRangeException();						
				
				if (!ShowSeconds)
					return;

				using (Selection.Saver savedSelection = new Selection.Saver(m_textBox))	// remember the current selection
				{					
					if (Second >= 0)		// see if there's already a second
						m_selection.Set(GetSecondStartPosition(), GetSecondStartPosition() + 2);
					
					m_selection.Replace(TwoDigits(value));	// set the second

					// Append the AM/PM if it's not in 24-hour format
					ShowAMPM();
				}
			}
		}

		/// <summary>
		///   Gets AM/PM symbol on the textbox. </summary>
		/// <remarks>
		///   If the AM/PM symbol is not valid or is not being shown on the textbox, this property will return an empty string. </remarks>
		/// <seealso cref="Hour" />
		/// <seealso cref="Minute" />
		/// <seealso cref="Second" />
		public string AMPM
		{
			get
			{
				string text = m_textBox.Text;
				int position = GetAMPMPosition(text);
				if (position > 0)
					return text.Substring(position);
				return "";
			}
		}

		/// <summary>
		///   Converts an hour in 12-hour format and its AM/PM symbol to its 
		///   24-hour equivalent. </summary>
		/// <param name="hour">
		///   The hour value to convert, in 12-hour format (1 to 12). </param>
		/// <param name="ampm">
		///   The AM/PM symbol which denotes if the hour is between 0 and 11, or 12 and 23. </param>
		/// <returns>
		///   The return value is the hour converted to 24-hour format (0 to 23). </returns>
		/// <seealso cref="ConvertToAMPMHour" />
		protected int ConvertTo24Hour(int hour, string ampm)
		{
			if (ampm == m_pm && hour >= 1 && hour <= 11)
				hour += 12;
			else if (ampm == m_am && hour == 12)
				hour = 0;
			return hour;
		}

		/// <summary>
		///   Converts an hour in 24-hour format to its 12-hour equivalent. </summary>
		/// <param name="hour">
		///   The hour value to convert, in 24-hour format (0 to 23). </param>
		/// <param name="ampm">
		///   The returned AM/PM symbol, used to denote if the hour was between 0 and 11, or 12 and 23. </param>
		/// <returns>
		///   The return value is the hour converted to 12-hour format (1 to 12). </returns>
		/// <seealso cref="ConvertTo24Hour" />
		protected int ConvertToAMPMHour(int hour, out string ampm)
		{
			ampm = m_am;

			if (hour >= 12)
			{
				hour -= 12;
				ampm = m_pm;
			}
			if (hour == 0)
				hour = 12;

			return hour;
		}

		/// <summary>
		///   Gets or sets the hour, minute, and second on the textbox using a <see cref="DateTime" /> object. </summary>
		/// <remarks>
		///   This property gets and sets the <see cref="DateTime" /> boxed inside an <c>object</c>.
		///   This makes it flexible, so that if the textbox does not hold a valid time, a <c>null</c> is returned, 
		///   instead of having to worry about an exception being thrown. </remarks>
		/// <example><code>
		///   object obj = txtTime.Behavior.Value;
		///   if (obj != null)
		///   {
		///     DateTime dtm = (DateTime)obj;
		///     ...
		///   } </code></example>
		/// <seealso cref="Hour" />
		/// <seealso cref="Minute" />
		/// <seealso cref="Second" />
		public object Value
		{
			get 
			{
				try
				{
					if (Show24HourFormat)
						return new DateTime(1900, 1, 1, Hour, Minute, GetValidSecond());
					return new DateTime(1900, 1, 1, ConvertTo24Hour(Hour, AMPM), Minute, GetValidSecond());
				}
				catch
				{
					return null;
				}
			}
			set
			{
				DateTime dt = (DateTime)value;
				m_textBox.Text = GetFormattedTime(dt.Hour, dt.Minute, dt.Second, "");
			}
		}

		/// <summary>
		///   Sets the hour, minute, and second on the textbox. </summary>
		/// <param name="hour">
		///   The hour to set, between 0 and 23. </param>
		/// <param name="minute">
		///   The minute to set, between 0 and 59. </param>
		/// <param name="second">
		///   The second to set, between 0 and 59. </param>
		/// <remarks>
		///   This is a convenience method to set each value individually using a single method. 
		///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. </remarks>
		public void SetTime(int hour, int minute, int second)
		{
			Value = new DateTime(1900, 1, 1, hour, minute, second);
		}
		
		/// <summary>
		///   Sets the hour and minute on the textbox. </summary>
		/// <param name="hour">
		///   The hour to set, between 0 and 23. </param>
		/// <param name="minute">
		///   The minute to set, between 0 and 59. </param>
		/// <remarks>
		///   This is a convenience method to set each value individually using a single method. 
		///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. 
		///   If the second is being shown on the textbox, it is set to 0.  </remarks>
		public void SetTime(int hour, int minute)
		{
			SetTime(hour, minute, 0);
		}
		
		/// <summary>
		///   Checks if the textbox's time is valid and falls within the allowed range. </summary>
		/// <returns>
		///   If the value is valid and falls within the allowed range, the return value is true; otherwise it is false. </returns>
		/// <seealso cref="RangeMin" />
		/// <seealso cref="RangeMax" />
		public override bool IsValid()
		{
			return IsValid(true);
		}

		/// <summary>
		///   Checks if the textbox's time is valid and optionally if it falls within the allowed range. </summary>
		/// <param name="checkRangeAlso">
		///   If true, the time is also checked that it falls within allowed range. </param>
		/// <returns>
		///   If the value is valid, the return value is true; otherwise it is false. </returns>
		/// <seealso cref="RangeMin" />
		/// <seealso cref="RangeMax" />
		public bool IsValid(bool checkRangeAlso)
		{
			// Check that we have a valid hour and minute
			int hour = Hour;
			int minute = Minute;
			if (hour < 0 || minute < 0)
				return false;

			// Check that the seconds are valid if being shown
			int second = Second;
			bool showingSeconds = ShowSeconds;
			if (showingSeconds != (second >= 0))
				return false;

			// Check the AM/PM portion
			string ampm = AMPM;
			bool force24HourFormat = Show24HourFormat;
			if ((force24HourFormat && ampm != "") ||
				(!force24HourFormat && (ampm != m_am && ampm != m_pm)))
				return false;

			if (!force24HourFormat && ampm == m_pm)
			{
				hour += 12;
				if (hour == 24)
					hour = 0;
			}
			if (!showingSeconds)
				second = m_rangeMin.Second; // avoids possible problem when checking range below

			// Check the range if desired
			if (checkRangeAlso)
				return IsWithinRange(new DateTime(1900, 1, 1, hour, minute, second));
			return true;
		}

		/// <summary>
		///   Gets the error message used to notify the user to enter a valid time value 
		///   within the allowed range. </summary>
		/// <seealso cref="IsValid" />
		public override string ErrorMessage
		{
			get
			{
				return "Please specify a time between " + GetFormattedTime(m_rangeMin.Hour, m_rangeMin.Minute, m_rangeMin.Second, "") + " and " + GetFormattedTime(m_rangeMax.Hour, m_rangeMax.Minute, m_rangeMax.Second, "") + ".";
			}
		}

		/// <summary>
		///   Gets or sets the minimum value allowed. </summary>
		/// <remarks>
		///   By default, this property is set to DateTime(1900, 1, 1, 0, 0, 0),
		///   however the range is only checked when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
		/// <seealso cref="RangeMax" />
		public DateTime RangeMin
		{
			get 
			{ 
				return m_rangeMin; 
			}
			set 
			{ 
				m_rangeMin = new DateTime(1900, 1, 1, value.Hour, value.Minute, value.Second); 
			}
		}

		/// <summary>
		///   Gets or sets the maximum value allowed. </summary>
		/// <remarks>
		///   By default, this property is set to DateTime(1900, 1, 1, 23, 59, 59),
		///   however the range is only checked when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
		/// <seealso cref="RangeMin" />
		public DateTime RangeMax
		{
			get 
			{ 
				return m_rangeMax; 
			}
			set 
			{ 
				m_rangeMax = new DateTime(1900, 1, 1, value.Hour, value.Minute, value.Second);	
			}
		}

		/// <summary>
		///   Checks if a time value falls within the allowed range. </summary>
		/// <param name="dt">
		///   The time value to check. </param>
		/// <returns>
		///   If the value is within the allowed range, the return value is true; otherwise it is false. </returns>
		/// <remarks>
		///   Only the time portion is checked; the date is ignored. </remarks>
		/// <seealso cref="RangeMin" />
		/// <seealso cref="RangeMax" />
		/// <seealso cref="IsValid" />
		public bool IsWithinRange(DateTime dt)
		{
			DateTime time = new DateTime(1900, 1, 1, dt.Hour, dt.Minute, dt.Second);
			return (time >= m_rangeMin && time <= m_rangeMax);
		}
		
		/// <summary>
		///   Gets or sets the character used to separate the hour, minute, and second values of the time. </summary>
		/// <remarks>
		///   By default, this property is set according to the user's system. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		public char Separator
		{
			get 
			{ 
				return m_separator; 
			}
			set
			{
				if (m_separator == value)
					return;
				
				Debug.Assert(value != 0);
				Debug.Assert(!Char.IsDigit(value));

				m_separator = value;
				UpdateText();
			}
		}

		/// <summary>
		///   Gets or sets whether the hour should be shown in 24-hour format. </summary>
		/// <remarks>
		///   By default, this property is set according to the user's system. 
		///   If the format is set to 12-hour format the AM/PM symbols are also shown; otherwise they are are not shown.
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="Flag.TwentyFourHourFormat" />
		public bool Show24HourFormat
		{
			get 
			{ 
				return HasFlag((int)Flag.TwentyFourHourFormat); 
			}
			set 
			{ 
				ModifyFlags((int)Flag.TwentyFourHourFormat, value);	
			}
		}

		/// <summary>
		///   Gets or sets whether the seconds should be shown (after the minutes). </summary>
		/// <remarks>
		///   By default, this property is set to false, so that the seconds are not shown. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="Flag.WithSeconds" />
		public bool ShowSeconds
		{
			get 
			{ 
				return HasFlag((int)Flag.WithSeconds); 
			}
			set 
			{ 
				ModifyFlags((int)Flag.WithSeconds, value);	
			}
		}

		/// <summary>
		///   Sets the symbols to use for AM and PM. </summary>
		/// <param name="am">
		///   The symbol to use for AM. </param>
		/// <param name="pm">
		///   The symbol to use for PM. </param>
		/// <exception cref="ArgumentException">The lengths of the parameters are not the same. </exception>
		/// <remarks>
		///   By default, the symbols are set according to the user's system. 
		///   This method allows them to be changed, however, they must both be identical in length.
		///   If the symbols are changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="GetAMPMSymbols" />
		public void SetAMPMSymbols(string am, string pm)
		{
			if (m_am == am && m_pm == pm)
				return;

			// Make sure they're the same length
			if (am.Length != pm.Length)
				throw new ArgumentException("The length of the AM and PM symbols must be identical.");

			m_am = am;
			m_pm = pm;

			if (m_am == "")
				m_am = "AM";
			if (m_pm == "")
				m_pm = "PM";

			m_ampmLength = m_am.Length;
			UpdateText();
		}

		/// <summary>
		///   Gets the symbols used for AM and PM. </summary>
		/// <param name="am">
		///   The symbol used for AM. </param>
		/// <param name="pm">
		///   The symbol used for PM. </param>
		/// <seealso cref="SetAMPMSymbols" />
		public void GetAMPMSymbols(out string am, out string pm)
		{
			am = m_am;
			pm = m_pm;
		}

		/// <summary>
		///   Retrieves the textbox's text in valid form. </summary>
		/// <returns>
		///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
		protected override string GetValidText()
		{
			string text = m_textBox.Text;
			
			// If it's empty or has a valid time, return it
			if (text == "")
				return text;

			if (IsValid(false))
				return GetFormattedTime(Hour, Minute, Second, AMPM);

			// If the hour, minute, and second are invalid, set it to the current time
			if (Hour < 0 && Minute < 0 && Second < 0)
			{
				DateTime dt = DateTime.Now;					
				return GetFormattedTime(dt.Hour, dt.Minute, dt.Second, "");
			}

			// Otherwise retrieve the validated time
			return GetFormattedTime(GetValidHour(true), GetValidMinute(), GetValidSecond(), AMPM);
		}

		/// <summary>
		///   Formats an hour, minute, second, and AM/PM value into a string based on the proper format. </summary>
		/// <param name="hour">
		///   The hour value. </param>
		/// <param name="minute">
		///   The minute value. </param>
		/// <param name="second">
		///   The second value. </param>
		/// <param name="ampm">
		///   The AM/PM value, which may be empty if the hour is in 24-hour format. </param>
		/// <returns>
		///   The return value is the formatted time value. </returns>
		public string GetFormattedTime(int hour, int minute, int second, string ampm)
		{
			if (Show24HourFormat)
			{
				// Handle switching from AM/PM to 24-hour format
				if (IsValidAMPM(ampm))
					hour = ConvertTo24Hour(hour, ampm);
			}
			else
			{
				// Handle switching from 24-hour format to AM/PM
				if (!IsValidAMPM(ampm))
					hour = ConvertToAMPMHour(hour, out ampm);
			}

			if (ShowSeconds)
			{
				if (Show24HourFormat)
					return String.Format("{0,2:00}{1}{2,2:00}{3}{4,2:00}", hour, m_separator, minute, m_separator, second);
				return String.Format("{0,2:00}{1}{2,2:00}{3}{4,2:00} {5}", hour, m_separator, minute, m_separator, second, ampm);
			}

			if (Show24HourFormat)
				return String.Format("{0,2:00}{1}{2,2:00}", hour, m_separator, minute);
			return String.Format("{0,2:00}{1}{2,2:00} {3}", hour, m_separator, minute, ampm);
		}

		/// <summary>
		///   Adjusts the textbox's value to be within the range of allowed values. </summary>
		protected void AdjustWithinRange()
		{
			// Check if it's already within the range
			if (IsValid())
				return;

			// If it's empty, set it to the current time
			if (m_textBox.Text == "")
				m_textBox.Text = " ";
			else
				UpdateText();

			// Make it fall within the range
			DateTime date = (DateTime)Value;
			if (date < m_rangeMin)
				Value = m_rangeMin;
			else if (date > m_rangeMax)
				Value = m_rangeMax;
		}
	}
	
	
	/////////////////////////////////////////////////////////////////////////////
	// DateTime behavior

	/// <summary>
	///   Behavior class which handles input of date and time values. </summary>
	/// <remarks>
	///   This behavior is designed to let the user enter a date and time value quickly and accurately.  
	///   As the user enters the digits, the separators are automatically filled in. The user may only remove 
	///   characters from the right side of the value entered. This helps to keep the value properly formatted. 
	///   The user may also use the up/down arrow keys to increment/decrement the month, day, year, hour, minute, or second
	///   depending on the location of the caret.  </remarks>
	/// <seealso cref="DateBehavior" />
	/// <seealso cref="TimeBehavior" />
	public class DateTimeBehavior : TimeBehavior
	{
		// Fields
		private DateBehavior m_dateBehavior;

		/// <summary>
		///   Internal values that are added/removed to the <see cref="Behavior.Flags" /> property by other
		///   properties of this class. </summary>
		[Flags]
		protected new enum Flag
		{
			/// <summary> Makes this object behave like the Date behavior, where only the date part is shown. </summary>
			/// <seealso cref="DateBehavior" />
			DateOnly									= 0x00100000,

			/// <summary> Makes this object behave like the Time behavior, where only the time part is shown. </summary>
			/// <seealso cref="TimeBehavior" />
			TimeOnly									= 0x00200000,

			/// <summary> The day is displayed in front of the month. </summary>
			/// <seealso cref="ShowDayBeforeMonth" />
			DayBeforeMonth								= 0x00010000,

			/// <summary> The hour is shown in 24-hour format (00 to 23). </summary>
			/// <seealso cref="TimeBehavior.Show24HourFormat" />
			TwentyFourHourFormat						= 0x00020000,

			/// <summary> The seconds are also shown. </summary>
			/// <seealso cref="TimeBehavior.ShowSeconds" />
			WithSeconds									= 0x00040000
		};

		/// <summary>
		///   Initializes a new instance of the DateTimeBehavior class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
		/// <exception cref="ArgumentNullException">textBox is null. </exception>
		/// <remarks>
		///   This constructor retrieves many of the properties from the user's system. </remarks>
		/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
		public DateTimeBehavior(TextBoxBase textBox) :
			base(textBox)
		{ 
			m_dateBehavior = new DateBehavior(textBox, false);  // does not add the event handlers
			m_flags |= m_dateBehavior.Flags;
			m_hourStart = 11;
		}

		/// <summary>
		///   Initializes a new instance of the DateTimeBehavior class by copying it from 
		///   another DateTimeBehavior object. </summary>
		/// <param name="behavior">
		///   The DateTimeBehavior object to copied (and then disposed of).  It must not be null. </param>
		/// <exception cref="ArgumentNullException">behavior is null. </exception>
		/// <remarks>
		///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
		public DateTimeBehavior(DateTimeBehavior behavior) :
			base(behavior)
		{
			m_dateBehavior = new DateBehavior(m_textBox, false);  // does not add the event handlers
		}

		/// <summary>
		///   Sets the month, day, year, hour, minute, and second on the textbox. </summary>
		/// <param name="year">
		///   The year to set. </param>
		/// <param name="month">
		///   The month to set. </param>
		/// <param name="day">
		///   The day to set. </param>
		/// <param name="hour">
		///   The hour to set, between 0 and 23. </param>
		/// <param name="minute">
		///   The minute to set, between 0 and 59. </param>
		/// <remarks>
		///   This is a convenience method to set each value individually using a single method. 
		///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. 
		///   If the second is being shown on the textbox, it is set to 0.  </remarks>
		public void SetDateTime(int year, int month, int day, int hour, int minute)
		{
			SetDateTime(year, month, day, hour, minute, 0);
		}

		/// <summary>
		///   Sets the month, day, year, hour, minute, and second on the textbox. </summary>
		/// <param name="year">
		///   The year to set. </param>
		/// <param name="month">
		///   The month to set. </param>
		/// <param name="day">
		///   The day to set. </param>
		/// <param name="hour">
		///   The hour to set, between 0 and 23. </param>
		/// <param name="minute">
		///   The minute to set, between 0 and 59. </param>
		/// <param name="second">
		///   The second to set, between 0 and 59. </param>
		/// <remarks>
		///   This is a convenience method to set each value individually using a single method. 
		///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. </remarks>
		public void SetDateTime(int year, int month, int day, int hour, int minute, int second)
		{
			if (HasFlag((int)Flag.DateOnly))
				m_dateBehavior.SetDate(year, month, day);
			else if (HasFlag((int)Flag.TimeOnly))
				SetTime(hour, minute, second);
			else
			{
				Debug.Assert(m_dateBehavior.IsWithinRange(new DateTime(year, month, day))); 
				m_textBox.Text = m_dateBehavior.GetFormattedDate(year, month, day) + ' ' + GetFormattedTime(hour, minute, second, "");
			}
		}

		/// <summary>
		///   Sets the month, day, and year on the textbox. </summary>
		/// <param name="year">
		///   The year to set. </param>
		/// <param name="month">
		///   The month to set. </param>
		/// <param name="day">
		///   The day to set. </param>
		/// <remarks>
		///   This is a convenience method to set each value individually using a single method. 
		///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. </remarks>
		public void SetDate(int year, int month, int day)
		{
			if (HasFlag((int)Flag.DateOnly) || !HasFlag((int)Flag.TimeOnly))
				m_dateBehavior.SetDate(year, month, day);
		}		

		/// <summary>
		///   Sets the hour, minute, and second on the textbox. </summary>
		/// <param name="hour">
		///   The hour to set, between 0 and 23. </param>
		/// <param name="minute">
		///   The minute to set, between 0 and 59. </param>
		/// <param name="second">
		///   The second to set, between 0 and 59. </param>
		/// <remarks>
		///   This is a convenience method to set each value individually using a single method. 
		///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. </remarks>
		public new void SetTime(int hour, int minute, int second)
		{
			if (!HasFlag((int)Flag.DateOnly) && HasFlag((int)Flag.TimeOnly))
				base.SetTime(hour, minute, second);
		}
		
		/// <summary>
		///   Sets the hour and minute on the textbox. </summary>
		/// <param name="hour">
		///   The hour to set, between 0 and 23. </param>
		/// <param name="minute">
		///   The minute to set, between 0 and 59. </param>
		/// <remarks>
		///   This is a convenience method to set each value individually using a single method. 
		///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. 
		///   If the second is being shown on the textbox, it is set to 0.  </remarks>
		public new void SetTime(int hour, int minute)
		{
			SetTime(hour, minute, 0);
		}
		
		/// <summary>
		///   Gets or sets the month, day, year, hour, minute, and second on the textbox using a <see cref="DateTime" /> object. </summary>
		/// <remarks>
		///   This property gets and sets the <see cref="DateTime" /> boxed inside an <c>object</c>.
		///   This makes it flexible, so that if the textbox does not hold a valid date and time, a <c>null</c> is returned, 
		///   instead of having to worry about an exception being thrown. </remarks>
		/// <example><code>
		///   object obj = txtDateTime.Behavior.Value;
		///   if (obj != null)
		///   {
		///     DateTime dtm = (DateTime)obj;
		///     ...
		///   } </code></example>
		/// <seealso cref="Month" />
		/// <seealso cref="Day" />
		/// <seealso cref="Year" />
		/// <seealso cref="TimeBehavior.Hour" />
		/// <seealso cref="TimeBehavior.Minute" />
		/// <seealso cref="TimeBehavior.Second" />
		public new object Value
		{
			get 
			{
				try
				{
					if (HasFlag((int)Flag.DateOnly))
						return m_dateBehavior.Value;
					if (HasFlag((int)Flag.TimeOnly))
						return base.Value;
					return new DateTime(Year, Month, Day, Hour, Minute, Second);
				}
				catch
				{
					return null;
				}
			}
			set
			{
				DateTime dt = (DateTime)value;
				SetDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
			}
		}

		/// <summary>
		///   Gets or sets the month on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid month. </exception>
		/// <remarks>
		///   If the month is not being shown or is not valid on the textbox, this property will return 0.
		///   This property must be set with a month that falls within the allowed range. </remarks>
		/// <seealso cref="Day" />
		/// <seealso cref="Year" />
		public int Month
		{
			get 
			{
				if (HasFlag((int)Flag.TimeOnly))
					return 0;
				return m_dateBehavior.Month;
			}			
			set
			{
				if (!HasFlag((int)Flag.TimeOnly))
					m_dateBehavior.Month = value;
			}
		}

		/// <summary>
		///   Gets or sets the day on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid day. </exception>
		/// <remarks>
		///   If the day is not being shown or is not valid on the textbox, this property will return 0.
		///   This property must be set with a day that falls within the allowed range. </remarks>
		/// <seealso cref="Month" />
		/// <seealso cref="Year" />
		public int Day
		{
			get 
			{
				if (HasFlag((int)Flag.TimeOnly))
					return 0;
				return m_dateBehavior.Day;
			}			
			set
			{
				if (!HasFlag((int)Flag.TimeOnly))
					m_dateBehavior.Day = value;
			}
		}

		/// <summary>
		///   Gets or sets the year on the textbox. </summary>
		/// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid year. </exception>
		/// <remarks>
		///   If the year is not being shown or is not valid on the textbox, this property will return 0.
		///   This property must be set with a year that falls within the allowed range. </remarks>
		/// <seealso cref="Month" />
		/// <seealso cref="Day" />
		public int Year
		{
			get 
			{
				if (HasFlag((int)Flag.TimeOnly))
					return 0;
				return m_dateBehavior.Year;
			}			
			set
			{
				if (!HasFlag((int)Flag.TimeOnly))
					m_dateBehavior.Year = value;
			}
		}

		/// <summary>
		///   Checks if the textbox's date and/or time is valid and falls within the allowed range. </summary>
		/// <returns>
		///   If the value is valid and falls within the allowed range, the return value is true; otherwise it is false. </returns>
		/// <seealso cref="RangeMin" />
		/// <seealso cref="RangeMax" />
		public override bool IsValid()
		{
			if (HasFlag((int)Flag.DateOnly))
				return m_dateBehavior.IsValid();
			if (HasFlag((int)Flag.TimeOnly))
				return base.IsValid();
			return (m_dateBehavior.IsValid() && base.IsValid());
		}

		/// <summary>
		///   Gets or sets the minimum value allowed. </summary>
		/// <remarks>
		///   By default, this property is set to DateTime(1900, 1, 1, 0, 0, 0).
		///   The range is actively checked as the user enters the date but the time is only checked 
		///   when the control loses focus, if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
		/// <seealso cref="RangeMax" />
		public new DateTime RangeMin
		{
			get 
			{ 
				if (HasFlag((int)Flag.DateOnly))
					return m_dateBehavior.RangeMin;
				if (HasFlag((int)Flag.TimeOnly))
					return base.RangeMin;

				DateTime rangeMin = base.RangeMin;
				return new DateTime(m_dateBehavior.RangeMin.Year, m_dateBehavior.RangeMin.Month, m_dateBehavior.RangeMin.Day, rangeMin.Hour, rangeMin.Minute, rangeMin.Second);
			}
			set 
			{
				base.RangeMin = value;
				if (HasFlag((int)Flag.DateOnly) || !HasFlag((int)Flag.TimeOnly))
					m_dateBehavior.RangeMin = value;		// updates the control
			}
		}

		/// <summary>
		///   Gets or sets the maximum value allowed. </summary>
		/// <remarks>
		///   By default, this property is set to <see cref="DateTime.MaxValue" />.
		///   The range is actively checked as the user enters the date but the time is only checked 
		///   when the control loses focus, if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
		/// <seealso cref="RangeMin" />
		public new DateTime RangeMax
		{
			get 
			{ 
				if (HasFlag((int)Flag.DateOnly))
					return m_dateBehavior.RangeMax;
				if (HasFlag((int)Flag.TimeOnly))
					return base.RangeMax;
				
				DateTime rangeMax = base.RangeMax;
				return new DateTime(m_dateBehavior.RangeMax.Year, m_dateBehavior.RangeMax.Month, m_dateBehavior.RangeMax.Day, rangeMax.Hour, rangeMax.Minute, rangeMax.Second);
			}
			set 
			{
				base.RangeMax = value;
				if (HasFlag((int)Flag.DateOnly) || !HasFlag((int)Flag.TimeOnly))
					m_dateBehavior.RangeMax = value;		// updates the control
			}
		}

		/// <summary>
		///   Checks if a date and time value falls within the allowed range. </summary>
		/// <param name="dt">
		///   The date and time value to check. </param>
		/// <returns>
		///   If the value is within the allowed range, the return value is true; otherwise it is false. </returns>
		/// <seealso cref="RangeMin" />
		/// <seealso cref="RangeMax" />
		/// <seealso cref="IsValid" />
		public new bool IsWithinRange(DateTime dt)
		{
			if (HasFlag((int)Flag.DateOnly))
				return m_dateBehavior.IsWithinRange(dt);
			if (HasFlag((int)Flag.TimeOnly))
				return base.IsWithinRange(dt);
			return (m_dateBehavior.IsWithinRange(dt) && base.IsWithinRange(dt));
		}

		/// <summary>
		///   Gets or sets the character used to separate the month, day, and year values of the date. </summary>
		/// <remarks>
		///   By default, this property is set according to the user's system. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		public char DateSeparator
		{
			get 
			{ 
				return m_dateBehavior.Separator; 
			}
			set 
			{ 
				m_dateBehavior.Separator = value; 
			}
		}

		/// <summary>
		///   Gets or sets the character used to separate the hour, minute, and second values of the time. </summary>
		/// <remarks>
		///   By default, this property is set according to the user's system. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		public char TimeSeparator
		{
			get 
			{ 
				return base.Separator; 
			}
			set 
			{ 
				base.Separator = value; 
			}
		}

		/// <summary>
		///   Gets the character used to separate the date or time value. </summary>
		/// <remarks>
		///   If only the date is being shown, this property retrieves the <see cref="DateSeparator" />.
		///   If only the time is being shown, this property retrieves the <see cref="TimeSeparator" />.
		///   If both the date and time are being shown, this property retrieves a space character. </remarks>
		private new char Separator
		{
			get
			{
				if (HasFlag((int)Flag.DateOnly))
					return m_dateBehavior.Separator;
				if (HasFlag((int)Flag.TimeOnly))
					return base.Separator;
				return ' ';
			}
		}

		/// <summary>
		///   Gets or sets whether the day should be shown before the month or after it. </summary>
		/// <remarks>
		///   By default, this property is set according to the user's system. 
		///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
		/// <seealso cref="Flag.DayBeforeMonth" />
		public bool ShowDayBeforeMonth
		{
			get 
			{ 
				return HasFlag((int)Flag.DayBeforeMonth); 
			}
			set 
			{
				if (!HasFlag((int)Flag.TimeOnly))
					ModifyFlags((int)Flag.DayBeforeMonth, value);	
			}						
		}

		/// <summary>
		///   Gets or sets the flags associated with this object. </summary>
		/// <remarks>
		///   This property behaves like the one in the <see cref="Behavior.Flags">base class</see> 
		///   but is overriden to properly set the start position of the hour, in case the 
		///   <see cref="Flag.DateOnly" /> or <see cref="Flag.TimeOnly" /> flags are turned on/off. </remarks>
		/// <seealso cref="Behavior.ModifyFlags" />
		public override int Flags
		{
			get 
			{ 
				return m_flags; 
			}
			set
			{
				if (m_flags == value)
					return;
				
				m_flags = value;
				m_hourStart = ((value & (int)Flag.TimeOnly) != 0) ? 0 : 11;
				
				m_dateBehavior.Flags = value;  // should call UpdateText
			}
		}
	
		/// <summary>
		///   Retrieves the textbox's text in valid form. </summary>
		/// <returns>
		///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
		protected override string GetValidText()
		{
			// Check if we're showing the date only
			string date = m_dateBehavior.GetValidTextForDateTime();
			if (HasFlag((int)Flag.DateOnly))
				return date;

			// Check if we're showing the time only
			string time = base.GetValidText();
			if (HasFlag((int)Flag.TimeOnly))
				return time;

			string space = (date != "" && time != "" ? " " : "");
			return date + space + time;
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyDown event. </remarks>
		/// <seealso cref="Control.KeyDown" />
		protected override void HandleKeyDown(object sender, KeyEventArgs e)
		{
			TraceLine("DateTimeBehavior.HandleKeyDown " + e.KeyCode);	

			// Check if we're showing the time only
			if (HasFlag((int)Flag.TimeOnly))
			{
				base.HandleKeyDown(sender, e);
				return;
			}

			if (e.KeyCode != Keys.Delete)
				m_dateBehavior.HandleKeyEvent(sender, e);

			if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Delete) && !HasFlag((int)Flag.DateOnly))
				base.HandleKeyDown(sender, e);
		}

		/// <summary>
		///   Handles keyboard presses inside the textbox. </summary>
		/// <param name="sender">
		///   The object who sent the event. </param>
		/// <param name="e">
		///   The event data. </param>
		/// <remarks>
		///   This method is overriden from the Behavior class and it  
		///   handles the textbox's KeyPress event. </remarks>
		/// <seealso cref="Control.KeyPress" />
		protected override void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			TraceLine("DateTimeBehavior.HandleKeyPress " + e.KeyChar);	
			
			// Check to see if it's read only
			if (m_textBox.ReadOnly)
				return;

			m_noTextChanged = true;
					
			// Check if we're showing the date or the time only
			if (HasFlag((int)Flag.DateOnly))
			{
				m_dateBehavior.HandleKeyEvent(sender, e);
				return;
			}
			if (HasFlag((int)Flag.TimeOnly))
			{
				base.HandleKeyPress(sender, e);
				return;
			}

			char c = e.KeyChar;
			e.Handled = true;
		
			int start, end;
			m_selection.Get(out start, out end);

			string text = m_textBox.Text;
			int length = text.Length;

			if (start >= 0 && start <= 9)
			{
				m_dateBehavior.HandleKeyEvent(sender, e);
				ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}
			else if (start == 10)
			{
				m_dateBehavior.HandleKeyEvent(sender, e);

				int space = 0;
				if (c == ' ')
					space = 1;
				else
					space = (base.IsValidHourDigit(c, 0) || (base.IsValidHourDigit(c, 1) && length <= 11) ? 2 : 0);
				
				// If we need the space, enter it
				if (space != 0)
					m_selection.SetAndReplace(start, start + 1, " ");
				
				// If the space is to be preceded by a valid digit, "type" it in.
				if (space == 2)
					SendKeys.Send(c.ToString());
				else
					base.ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
			}
			else
				base.HandleKeyPress(sender, e);
		}

		/// <summary>
		///   Gets the error message used to notify the user to enter a valid date and time value 
		///   within the allowed range. </summary>
		/// <seealso cref="IsValid" />
		public override string ErrorMessage
		{
			get
			{
				// Get the message depending on what we're showing
				if (HasFlag((int)Flag.DateOnly))
					return m_dateBehavior.ErrorMessage;
				else if (HasFlag((int)Flag.TimeOnly))
					return base.ErrorMessage;
				else
				{
					string minDateTime = 
						m_dateBehavior.GetFormattedDate(m_dateBehavior.RangeMin.Year, m_dateBehavior.RangeMin.Month, m_dateBehavior.RangeMin.Day) + ' ' + 
						base.GetFormattedTime(base.RangeMin.Hour, base.RangeMin.Minute, base.RangeMin.Second, "");
					string maxDateTime = 
						m_dateBehavior.GetFormattedDate(m_dateBehavior.RangeMax.Year, m_dateBehavior.RangeMax.Month, m_dateBehavior.RangeMax.Day) + ' ' + 
						base.GetFormattedTime(base.RangeMax.Hour, base.RangeMax.Minute, base.RangeMax.Second, "");
													
					return "Please specify a date and time between " + minDateTime + " and " + maxDateTime + '.';
				}
			}
		}
	}
}
