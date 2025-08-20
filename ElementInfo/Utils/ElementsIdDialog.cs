using System;
using System.Windows.Forms;
using MagicEntry.Plugins.ElementInfo.Constants;

namespace MagicEntry.Plugins.ElementInfo.Utils
{
    // WinForms диалог для отображения ID элементов
    public partial class ElementsIdDialog : Form
    {
        private RichTextBox richTextBoxIds;
        private Button buttonCopy;
        private Button buttonSelectMore;
        private Button buttonCancel;

        public ElementsIdDialog(string idsText)
        {
            InitializeComponent();
            richTextBoxIds.Text = idsText;
        }

        // Обновляет текст в RichTextBox
        public void UpdateText(string idsText)
        {
            richTextBoxIds.Text = idsText;
        }

        public void AppendText(string additionalText)
        {
            if (!string.IsNullOrEmpty(richTextBoxIds.Text))
            {
                richTextBoxIds.Text += " " + additionalText;
            }
            else
            {
                richTextBoxIds.Text = additionalText;
            }
        }

        private void InitializeComponent()
        {
            this.richTextBoxIds = new RichTextBox();
            this.buttonCopy = new Button();
            this.buttonSelectMore = new Button();
            this.buttonCancel = new Button();
            this.SuspendLayout();

            // 
            // richTextBoxIds
            // 
            this.richTextBoxIds.Location = new System.Drawing.Point(12, 12);
            this.richTextBoxIds.Name = "richTextBoxIds";
            this.richTextBoxIds.Size = new System.Drawing.Size(560, 200);
            this.richTextBoxIds.TabIndex = 0;
            this.richTextBoxIds.Text = "";
            this.richTextBoxIds.WordWrap = true;
            this.richTextBoxIds.Font = new System.Drawing.Font("Consolas", 10F);

            // 
            // buttonCopy
            // 
            this.buttonCopy.Location = new System.Drawing.Point(12, 230);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(180, 30);
            this.buttonCopy.TabIndex = 1;
            this.buttonCopy.Text = Messages.COPY_TO_CLIPBOARD_BUTTON;
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new EventHandler(this.ButtonCopy_Click);

            // 
            // buttonSelectMore
            // 
            this.buttonSelectMore.Location = new System.Drawing.Point(210, 230);
            this.buttonSelectMore.Name = "buttonSelectMore";
            this.buttonSelectMore.Size = new System.Drawing.Size(180, 30);
            this.buttonSelectMore.TabIndex = 2;
            this.buttonSelectMore.Text = Messages.SELECT_MORE_ELEMENTS_BUTTON;
            this.buttonSelectMore.UseVisualStyleBackColor = true;
            this.buttonSelectMore.Click += new EventHandler(this.ButtonSelectMore_Click);

            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(410, 230);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 30);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // 
            // ElementsIdDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 281);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSelectMore);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.richTextBoxIds);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ElementsIdDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = Messages.ELEMENTS_ID_DIALOG_TITLE;
            this.ResumeLayout(false);
        }

        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(richTextBoxIds.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonSelectMore_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
        }
    }
}
