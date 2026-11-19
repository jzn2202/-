using System;
using System.Drawing;
using System.Windows.Forms;

namespace ReminderMini
{
    internal sealed partial class MainForm
    {
        private void BuildInterface()
        {
            GridSurface surface = new GridSurface();
            surface.Dock = DockStyle.Fill;
            surface.Padding = new Padding(26, 22, 26, 17);
            Controls.Add(surface);

            TableLayoutPanel page = new TableLayoutPanel();
            page.Dock = DockStyle.Fill;
            page.BackColor = Color.Transparent;
            page.ColumnCount = 1;
            page.RowCount = 3;
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
            surface.Controls.Add(page);

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Color.Transparent;
            page.Controls.Add(header, 0, 0);
            Label eyebrow = Label("―  单次倒计时", 9F, TechTheme.Cyan);
            eyebrow.SetBounds(4, 2, 300, 23);
            header.Controls.Add(eyebrow);
            headingLabel = Label("设置下一次提醒", 19F, TechTheme.Text, true);
            headingLabel.SetBounds(2, 29, 520, 40);
            header.Controls.Add(headingLabel);
            Label description = Label("让注意力留在当下，时间到了再处理下一件事。", 9F, TechTheme.Muted);
            description.SetBounds(4, 76, 610, 25);
            header.Controls.Add(description);
            statusLabel = Label("等待开始", 9F, TechTheme.Cyan, true);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            statusLabel.SetBounds(620, 34, 106, 28);
            header.Controls.Add(statusLabel);
            header.Resize += delegate { statusLabel.Left = header.ClientSize.Width - statusLabel.Width - 4; };

            TableLayoutPanel columns = new TableLayoutPanel();
            columns.Dock = DockStyle.Fill;
            columns.BackColor = Color.Transparent;
            columns.ColumnCount = 2;
            columns.RowCount = 1;
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
            page.Controls.Add(columns, 0, 1);

            TechCard timerCard = new TechCard();
            timerCard.Dock = DockStyle.Fill;
            timerCard.Margin = new Padding(0, 0, 10, 0);
            timerCard.Padding = new Padding(19, 17, 19, 16);
            columns.Controls.Add(timerCard, 0, 0);
            TableLayoutPanel timerContent = new TableLayoutPanel();
            timerContent.Dock = DockStyle.Fill;
            timerContent.BackColor = Color.Transparent;
            timerContent.ColumnCount = 1;
            timerContent.RowCount = 3;
            timerContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
            timerContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            timerContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            timerCard.Controls.Add(timerContent);
            Label remainingLabel = Label("剩余时间", 8F, TechTheme.Dim);
            remainingLabel.Dock = DockStyle.Fill;
            timerContent.Controls.Add(remainingLabel, 0, 0);
            ring = new RingDisplay();
            ring.Dock = DockStyle.Fill;
            ring.Margin = new Padding(0, 3, 0, 5);
            timerContent.Controls.Add(ring, 0, 1);
            TableLayoutPanel timerFoot = new TableLayoutPanel();
            timerFoot.Dock = DockStyle.Fill;
            timerFoot.BackColor = Color.Transparent;
            timerFoot.ColumnCount = 2;
            timerFoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            timerFoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            timerContent.Controls.Add(timerFoot, 0, 2);
            Label once = Label("单次倒计时", 8F, TechTheme.Muted);
            once.Dock = DockStyle.Fill;
            once.TextAlign = ContentAlignment.BottomLeft;
            timerFoot.Controls.Add(once, 0, 0);
            durationHintLabel = Label("40 分钟", 8F, TechTheme.Text);
            durationHintLabel.Dock = DockStyle.Fill;
            durationHintLabel.TextAlign = ContentAlignment.BottomRight;
            timerFoot.Controls.Add(durationHintLabel, 1, 0);

            Panel rightHost = new Panel();
            rightHost.Dock = DockStyle.Fill;
            rightHost.Margin = new Padding(10, 0, 0, 0);
            rightHost.BackColor = Color.Transparent;
            columns.Controls.Add(rightHost, 1, 0);
            setupPanel = BuildSetupPanel();
            runningPanel = BuildRunningPanel();
            rightHost.Controls.Add(setupPanel);
            rightHost.Controls.Add(runningPanel);

            TableLayoutPanel footer = new TableLayoutPanel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor = Color.Transparent;
            footer.ColumnCount = 2;
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            page.Controls.Add(footer, 0, 2);
            Label trayHint = Label("▣  关闭窗口后仍在托盘计时", 8F, TechTheme.Dim);
            trayHint.Dock = DockStyle.Fill;
            trayHint.TextAlign = ContentAlignment.BottomLeft;
            footer.Controls.Add(trayHint, 0, 0);
            Label localHint = Label("本地保存 · 无需联网", 8F, TechTheme.Dim);
            localHint.Dock = DockStyle.Fill;
            localHint.TextAlign = ContentAlignment.BottomRight;
            footer.Controls.Add(localHint, 1, 0);

        }

        private Panel BuildSetupPanel()
        {
            TechCard card = new TechCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(21, 18, 21, 16);
            TableLayoutPanel layout = CardLayout(card, 9);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 51));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 47));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));

            setDurationButton = ActionButton("◷  设置倒计时", TechButtonKind.Secondary,
                OnSetDurationClicked);
            setDurationButton.Dock = DockStyle.Fill;
            setDurationButton.Margin = new Padding(0, 0, 0, 4);
            layout.Controls.Add(setDurationButton, 0, 0);
            selectedDurationLabel = Label("当前时长  ·  40 分钟", 9F, TechTheme.Cyan);
            selectedDurationLabel.Dock = DockStyle.Fill;
            layout.Controls.Add(selectedDurationLabel, 0, 1);

            Label messageTitle = Label("提醒内容（可选）", 9F, TechTheme.Muted);
            messageTitle.Dock = DockStyle.Fill;
            layout.Controls.Add(messageTitle, 0, 3);
            DarkTextArea messageArea = new DarkTextArea(true);
            messageArea.Dock = DockStyle.Fill;
            messageArea.Margin = new Padding(0, 3, 0, 9);
            messageBox = messageArea.Editor;
            layout.Controls.Add(messageArea, 0, 4);

            soundBox = new TechCheckBox();
            soundBox.Text = "到点播放提示音";
            soundBox.ForeColor = TechTheme.Text;
            soundBox.BackColor = Color.Transparent;
            soundBox.Dock = DockStyle.Fill;
            soundBox.Padding = new Padding(0, 0, 0, 0);
            layout.Controls.Add(soundBox, 0, 5);
            startButton = ActionButton("▷  启动倒计时", TechButtonKind.Primary, OnStartClicked);
            startButton.Dock = DockStyle.Fill;
            layout.Controls.Add(startButton, 0, 6);
            validationLabel = Label("", 8F, TechTheme.Danger);
            validationLabel.Dock = DockStyle.Fill;
            layout.Controls.Add(validationLabel, 0, 7);
            return card;
        }

        private Panel BuildRunningPanel()
        {
            TechCard card = new TechCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(21, 18, 21, 16);
            TableLayoutPanel layout = CardLayout(card, 7);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 138));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 57));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 2));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
            Label title = Label("当前提醒", 14F, TechTheme.Text, true);
            title.Dock = DockStyle.Fill;
            layout.Controls.Add(title, 0, 0);
            Label sub = Label("到点时显示", 9F, TechTheme.Muted);
            sub.Dock = DockStyle.Fill;
            layout.Controls.Add(sub, 0, 1);
            DarkTextArea runMessageArea = new DarkTextArea(true);
            runMessageBox = runMessageArea.Editor;
            runMessageBox.ReadOnly = true;
            runMessageBox.TabStop = false;
            runMessageArea.Dock = DockStyle.Fill;
            runMessageArea.Margin = new Padding(0, 0, 0, 12);
            layout.Controls.Add(runMessageArea, 0, 2);

            TableLayoutPanel facts = new TableLayoutPanel();
            facts.Dock = DockStyle.Fill;
            facts.BackColor = Color.Transparent;
            facts.ColumnCount = 2;
            facts.RowCount = 3;
            facts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            facts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
            for (int i = 0; i < 3; i++) facts.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            layout.Controls.Add(facts, 0, 3);
            Fact(facts, 0, "计时状态", out runStatusLabel);
            Label soundValue;
            Fact(facts, 1, "提醒方式", out soundValue);
            soundStatusLabel = soundValue;
            Label placeValue;
            Fact(facts, 2, "运行位置", out placeValue);
            placeValue.Text = "窗口 / 系统托盘";

            TableLayoutPanel actions = new TableLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.BackColor = Color.Transparent;
            actions.ColumnCount = 3;
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.Controls.Add(actions, 0, 4);
            pauseButton = ActionButton("暂停", TechButtonKind.Primary, OnPauseClicked);
            resetButton = ActionButton("重置", TechButtonKind.Secondary, OnResetClicked);
            endButton = ActionButton("结束", TechButtonKind.Danger, OnEndClicked);
            pauseButton.Dock = resetButton.Dock = endButton.Dock = DockStyle.Fill;
            pauseButton.Margin = new Padding(0, 6, 5, 0);
            resetButton.Margin = new Padding(3, 6, 5, 0);
            endButton.Margin = new Padding(3, 6, 0, 0);
            actions.Controls.Add(pauseButton, 0, 0);
            actions.Controls.Add(resetButton, 1, 0);
            actions.Controls.Add(endButton, 2, 0);
            return card;
        }

        private static TableLayoutPanel CardLayout(Panel card, int rows)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.BackColor = Color.Transparent;
            layout.ColumnCount = 1;
            layout.RowCount = rows;
            card.Controls.Add(layout);
            return layout;
        }

        private static void Fact(TableLayoutPanel table, int row, string name, out Label value)
        {
            Label label = Label(name, 8.5F, TechTheme.Muted);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            table.Controls.Add(label, 0, row);
            value = Label("", 8.5F, TechTheme.Text, true);
            value.Dock = DockStyle.Fill;
            value.TextAlign = ContentAlignment.MiddleRight;
            table.Controls.Add(value, 1, row);
        }

        private static Label Label(string text, float size, Color color, bool bold = false)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                BackColor = Color.Transparent,
                ForeColor = color,
                Font = new Font("Microsoft YaHei UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Button ActionButton(string text, TechButtonKind kind, EventHandler handler)
        {
            TechButton button = new TechButton();
            button.Text = text;
            button.Kind = kind;
            button.Click += handler;
            return button;
        }

    }
}
