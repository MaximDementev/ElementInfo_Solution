using System;
using System.Drawing;
using System.Windows.Forms;

namespace MagicEntry.Plugins.ElementInfo.Utils
{
    // WinForms диалог для ввода текста с валидацией
    public partial class TextInputForm : Form
    {
        public string InputText { get; private set; }

        public TextInputForm(string title, string message, string invalidText = "")
        {
            InitializeComponent();
            this.Text = title;
            labelMessage.Text = message;

            // Если передан невалидный текст, показываем его в поле ввода
            if (!string.IsNullOrEmpty(invalidText))
            {
                textBoxInput.Text = invalidText;
                textBoxInput.SelectAll();
            }
        }

        private void InitializeComponent()
        {
            this.labelMessage = new Label();
            this.textBoxInput = new TextBox();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.SuspendLayout();

            // labelMessage
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new Point(12, 15);
            this.labelMessage.MaximumSize = new Size(360, 0);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new Size(35, 13);
            this.labelMessage.TabIndex = 0;
            this.labelMessage.Text = "Сообщение";

            // textBoxInput
            this.textBoxInput.Location = new Point(15, 40);
            this.textBoxInput.Multiline = true;
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.ScrollBars = ScrollBars.Vertical;
            this.textBoxInput.Size = new Size(357, 100);
            this.textBoxInput.TabIndex = 1;

            // buttonOK
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Location = new Point(216, 155);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new EventHandler(this.buttonOK_Click);

            // buttonCancel
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(297, 155);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;

            // TextInputForm
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(384, 190);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxInput);
            this.Controls.Add(this.labelMessage);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TextInputForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Ввод текста";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            InputText = textBoxInput.Text;
        }

        private Label labelMessage;
        private TextBox textBoxInput;
        private Button buttonOK;
        private Button buttonCancel;
    }
}
