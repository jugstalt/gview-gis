using System;
using System.Windows.Forms;
using System.Diagnostics;


namespace gView.Framework.UI.Controls
{
	/////////////////////////////////////////////////////////////////////////////
	// Selection
	
	/// <summary>
	///   Encapsulates a textbox's selection. </summary>
	/// <seealso cref="Saver" />
	public class Selection
	{
		// Fields
		private TextBoxBase m_textBox;

		/// <summary>
		///   Event used to notify that the selected text is about to change. </summary>
		/// <remarks>
		///   This event is fired by Replace right before it replaces the textbox's text. </remarks>
		/// <seealso cref="Replace" />
		public event EventHandler TextChanging;
		
		/// <summary>
		///   Initializes a new instance of the Selection class by associating it with a TextBoxBase derived object. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object for which the selection is being manipulated. </param>
		/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
		public Selection(TextBoxBase textBox)
		{
			m_textBox = textBox;
		}

		/// <summary>
		///   Initializes a new instance of the Selection class by associating it with a TextBoxBase derived object. 
		///   and then selecting text on it. </summary>
		/// <param name="textBox">
		///   The TextBoxBase object for which the selection is being manipulated. </param>
		/// <param name="start">
		///   The zero-based position where to start the selection. </param>
		/// <param name="end">
		///   The zero-based position where to end the selection.  If it's equal to the start position, no text is selected. </param>
		/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
		public Selection(TextBoxBase textBox, int start, int end)
		{
			m_textBox = textBox;
			Set(start, end);
		}

		/// <summary>
		///   Selects the textbox's text. </summary>
		/// <param name="start">
		///   The zero-based position where to start the selection. </param>
		/// <param name="end">
		///   The zero-based position where to end the selection.  If it's equal to the start position, no text is selected. </param>
		/// <remarks>
		///   The end must be greater than or equal to the start position. </remarks>
		/// <seealso cref="Get" />
		public void Set(int start, int end)
		{
			m_textBox.SelectionStart = start;
			m_textBox.SelectionLength = end - start;
		}		
		
		/// <summary>
		///   Retrieves the start and end position of the selection. </summary>
		/// <param name="start">
		///   The zero-based position where the selection starts. </param>
		/// <param name="end">
		///   The zero-based position where selection ends.  If it's equal to the start position, no text is selected. </param>
		/// <seealso cref="Set" />
		public void Get(out int start, out int end)
		{
			start = m_textBox.SelectionStart;
			end = start + m_textBox.SelectionLength;
			
			if (start < 0)
				start = 0;
			if (end < start)
				end = start;
		}
				
		/// <summary>
		///   Replaces the text selected on the textbox. </summary>
		/// <param name="text">
		///   The text to replace the selection with. </param>
		/// <remarks>
		///   If nothing is selected, the text is inserted at the caret's position. </remarks>
		/// <seealso cref="SetAndReplace" />
		public void Replace(string text)
		{
			if (TextChanging != null)
				TextChanging(this, null);

			m_textBox.SelectedText = text;
		}

		/// <summary>
		///   Selects the textbox's text and replaces it. </summary>
		/// <param name="start">
		///   The zero-based position where to start the selection. </param>
		/// <param name="end">
		///   The zero-based position where to end the selection.  If it's equal to the start position, no text is selected. </param>
		/// <param name="text">
		///   The text to replace the selection with. </param>
		/// <remarks>
		///   The end must be greater than or equal to the start position.
		///   If nothing gets selected, the text is inserted at the caret's position. </remarks>
		/// <seealso cref="Set" />
		/// <seealso cref="Replace" />
		public void SetAndReplace(int start, int end, string text)
		{
			Set(start, end);
			Replace(text);
		}		

		/// <summary>
		///   Changes the selection's start and end positions by an offset. </summary>
		/// <param name="start">
		///   How much to change the start of the selection by. </param>
		/// <param name="end">
		///   How much to change the end of the selection by. </param>
		/// <seealso cref="Set" />	
		public void MoveBy(int start, int end)
		{
			End += end;
			Start += start;
		}
		
		/// <summary>
		///   Changes the internal start and end positions by an offset. </summary>
		/// <param name="pos">
		///   How much to change the start and end of the selection by. </param>
		/// <seealso cref="Set" />	
		public void MoveBy(int pos)
		{
			MoveBy(pos, pos);
		}

		/// <summary>
		///   Creates a new Selection object with the internal start and end 
		///   positions changed by an offset. </summary>
		/// <param name="selection">
		///   The object with the original selection.  </param>
		/// <param name="pos">
		///   How much to change the start and end of the selection by on the resulting object. </param>
		/// <seealso cref="MoveBy" />	
		/// <seealso cref="Set" />	
		public static Selection operator+(Selection selection, int pos)
		{
			return new Selection(selection.m_textBox, selection.Start + pos, selection.End + pos);
		}			
			
		/// <summary>
		///   Gets the TextBoxBase object associated with this Selection object. </summary>
		public TextBoxBase TextBox
		{
			get 
			{ 
				return m_textBox; 
			}
		}
		
		/// <summary>
		///   Gets or sets the zero-based position for the start of the selection. </summary>
		/// <seealso cref="End" />	
		/// <seealso cref="Length" />	
		public int Start
		{
			get 
			{ 
				return m_textBox.SelectionStart; 
			}
			set
			{
				m_textBox.SelectionStart = value;
			}
		}
		
		/// <summary>
		///   Gets or sets the zero-based position for the end of the selection. </summary>
		/// <seealso cref="Start" />	
		/// <seealso cref="Length" />	
		public int End
		{
			get 
			{ 
				return m_textBox.SelectionStart + m_textBox.SelectionLength; 
			}
			set
			{
				m_textBox.SelectionLength = value - m_textBox.SelectionStart;
			}
		}

		/// <summary>
		///   Gets or sets the length of the selection. </summary>
		/// <seealso cref="Start" />	
		/// <seealso cref="End" />	
		public int Length
		{
			get 
			{ 
				return m_textBox.SelectionLength; 
			}
			set
			{
				m_textBox.SelectionLength = value;
			}
		}

		/// <summary>
		///   Saves (and later restores) the current start and end position of a textbox selection. </summary>
		/// <remarks>
		///   This class saves the start and end position of the textbox with which it is constructed
		///   and then restores it when Restore is called.  Since this is a IDisposable class, it can also
		///   be used inside a <c>using</c> statement to Restore the selection (via Dispose). </remarks>
		public class Saver : IDisposable
		{
			// Fields
			private TextBoxBase m_textBox;
			private Selection m_selection;
			private int m_start, m_end;

			/// <summary>
			///   Initializes a new instance of the Saver class by associating it with a TextBoxBase derived object. </summary>
			/// <param name="textBox">
			///   The TextBoxBase object for which the selection is being saved. </param>
			/// <remarks>
			///   This constructor saves the textbox's start and end position of the selection inside private fields. </remarks>
			/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
			public Saver(TextBoxBase textBox)
			{
				m_textBox = textBox;
				m_selection = new Selection(textBox);
				m_selection.Get(out m_start, out m_end);			
			}

			/// <summary>
			///   Initializes a new instance of the Saver class by associating it with a TextBoxBase derived object 
			///   and passing the start and end position of the selection. </summary>
			/// <param name="textBox">
			///   The TextBoxBase object for which the selection is being saved. </param>
			/// <param name="start">
			///   The zero-based start position of the selection. </param>
			/// <param name="end">
			///   The zero-based end position of the selection. It must not be less than the start position. </param>
			/// <remarks>
			///   This constructor does not save the textbox's start and end position of the selection.
			///   Instead, it saves the two given parameters. </remarks>
			/// <seealso cref="System.Windows.Forms.TextBoxBase" />	
			public Saver(TextBoxBase textBox, int start, int end)
			{
				m_textBox = textBox;
				m_selection = new Selection(textBox);
				Debug.Assert(start <= end);

				m_start = start;
				m_end = end;
			}
			
			/// <summary>
			///   Restores the selection on the textbox to the saved start and end values. </summary>
			/// <remarks>
			///   This method checks that the textbox is still <see cref="Disable">available</see> 
			///   and if so restores the selection.  </remarks>
			/// <seealso cref="Disable" />	
			public void Restore()
			{
				if (m_textBox == null)
					return;
				
				m_selection.Set(m_start, m_end);
				m_textBox = null;
			}

			/// <summary>
			///   Calls the <see cref="Restore" /> method. </summary>
			public void Dispose()
			{
				Restore();
			}
			
			/// <summary>
			///   Changes the internal start and end positions. </summary>
			/// <param name="start">
			///   The new zero-based position for the start of the selection. </param>
			/// <param name="end">
			///   The new zero-based position for the end of the selection. It must not be less than the start position. </param>
			/// <seealso cref="MoveBy" />	
			public void MoveTo(int start, int end)
			{
				Debug.Assert(start <= end);
			
				m_start = start;
				m_end = end;
			}
			
			/// <summary>
			///   Changes the internal start and end positions by an offset. </summary>
			/// <param name="start">
			///   How much to change the start of the selection by. </param>
			/// <param name="end">
			///   How much to change the end of the selection by. </param>
			/// <seealso cref="MoveTo" />	
			public void MoveBy(int start, int end)
			{
				m_start += start;
				m_end += end;
			
				Debug.Assert(m_start <= m_end);
			}
			
			/// <summary>
			///   Changes the internal start and end positions by an offset. </summary>
			/// <param name="pos">
			///   How much to change the start and end of the selection by. </param>
			/// <seealso cref="MoveTo" />	
			public void MoveBy(int pos)
			{
				m_start += pos;
				m_end += pos;
			}
			
			/// <summary>
			///   Creates a new Saver object with the internal start and end 
			///   positions changed by an offset. </summary>
			/// <param name="saver">
			///   The object with the original saved selection.  </param>
			/// <param name="pos">
			///   How much to change the start and end of the selection by on the resulting object. </param>
			/// <seealso cref="MoveTo" />	
			public static Saver operator+(Saver saver, int pos)
			{
				return new Saver(saver.m_textBox, saver.m_start + pos, saver.m_end + pos);
			}
			
			/// <summary>
			///   Gets the TextBoxBase object associated with this Saver object. </summary>
			public TextBoxBase TextBox
			{
				get 
				{ 
					return m_textBox; 
				}
			}
		
			/// <summary>
			///   Gets or sets the zero-based position for the start of the selection. </summary>
			/// <seealso cref="End" />	
			public int Start
			{
				get 
				{ 
					return m_start; 
				}
				set
				{
					m_start = value;
				}
			}
			
			/// <summary>
			///   Gets or sets the zero-based position for the end of the selection. </summary>
			/// <seealso cref="Start" />	
			public int End
			{
				get 
				{ 
					return m_end; 
				}
				set
				{
					m_end = value;
				}
			}
			
			/// <summary>
			///   Updates the internal start and end positions with the current selection on the textbox. </summary>
			/// <seealso cref="Disable" />	
			public void Update()
			{
				if (m_textBox != null)
					m_selection.Get(out m_start, out m_end);
			}
			
			/// <summary>
			///   Disables restoring of the textbox's selection when <see cref="Dispose" /> is called. </summary>
			/// <seealso cref="Dispose" />	
			/// <seealso cref="Update" />	
			public void Disable()
			{
				m_textBox = null;
			}			
		}
	}
}
