using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace bai4810
{
    public partial class Form1 : Form
    {
        private TableLayoutPanel tlpSeats;
        private Label lblCount;
        private Label lblTotal;
        private ComboBox cmbTimeSlot;
        private Button btnConfirm;
        private Button btnClearAll;

        private int selectedCount = 0;

        private enum SeatState { Empty = 0, Selected = 1, Booked = 2 }

        private class SeatInfo
        {
            public int Index { get; set; }
            public SeatState State { get; set; }
        }

        private class TimeSlotItem
        {
            public string Name { get; }
            public int Price { get; }
            public TimeSlotItem(string name, int price) { Name = name; Price = price; }
            public override string ToString() => $"{Name} - {Price:N0}đ";
        }

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            this.Text = "Sơ đồ chọn vị trí / Đặt bàn hẹn giờ";
            this.ClientSize = new Size(900, 520);

            // Main TableLayout for seats
            tlpSeats = new TableLayoutPanel();
            tlpSeats.RowCount = 5;
            tlpSeats.ColumnCount = 4;
            tlpSeats.Dock = DockStyle.Left;
            tlpSeats.Width = 600;
            tlpSeats.Padding = new Padding(10);
            tlpSeats.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            for (int r = 0; r < tlpSeats.RowCount; r++)
                tlpSeats.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / tlpSeats.RowCount));
            for (int c = 0; c < tlpSeats.ColumnCount; c++)
                tlpSeats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / tlpSeats.ColumnCount));

            // Right panel for controls
            var pnlRight = new Panel();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Padding = new Padding(12);

            lblCount = new Label();
            lblCount.AutoSize = true;
            lblCount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblCount.Location = new Point(10, 10);
            lblCount.Text = "Số vị trí đang chọn: 0";

            lblTotal = new Label();
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTotal.Location = new Point(10, 40);
            lblTotal.Text = "Tạm tính tiền: 0đ";

            cmbTimeSlot = new ComboBox();
            cmbTimeSlot.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTimeSlot.Location = new Point(10, 80);
            cmbTimeSlot.Width = 220;
            cmbTimeSlot.Items.Add(new TimeSlotItem("Sáng", 100000));
            cmbTimeSlot.Items.Add(new TimeSlotItem("Tối", 150000));
            cmbTimeSlot.SelectedIndex = 0;
            cmbTimeSlot.SelectedIndexChanged += CmbTimeSlot_SelectedIndexChanged;

            btnConfirm = new Button();
            btnConfirm.Text = "Xác nhận đặt";
            btnConfirm.Location = new Point(10, 130);
            btnConfirm.Width = 140;
            btnConfirm.Click += BtnConfirm_Click;

            btnClearAll = new Button();
            btnClearAll.Text = "Hủy chọn tất cả";
            btnClearAll.Location = new Point(160, 130);
            btnClearAll.Width = 140;
            btnClearAll.Click += BtnClearAll_Click;

            pnlRight.Controls.Add(lblCount);
            pnlRight.Controls.Add(lblTotal);
            pnlRight.Controls.Add(cmbTimeSlot);
            pnlRight.Controls.Add(btnConfirm);
            pnlRight.Controls.Add(btnClearAll);

            this.Controls.Add(pnlRight);
            this.Controls.Add(tlpSeats);

            // Pre-booked indices (demo): these seats will be locked (booked)
            var preBooked = new HashSet<int> { 2, 5, 7 };

            // Create 20 buttons dynamically using for loop
            for (int i = 0; i < 20; i++)
            {
                var btn = new Button();
                btn.Dock = DockStyle.Fill;
                btn.Margin = new Padding(8);
                btn.Text = (i + 1).ToString();
                var info = new SeatInfo { Index = i, State = SeatState.Empty };

                if (preBooked.Contains(i))
                {
                    info.State = SeatState.Booked;
                    btn.BackColor = Color.IndianRed;
                    btn.ForeColor = Color.White;
                    btn.Enabled = true; // keep enabled to show tooltip or message
                }
                else
                {
                    btn.BackColor = Color.WhiteSmoke;
                    btn.ForeColor = Color.Black;
                }

                btn.Tag = info;
                btn.Click += SeatButton_Click; // shared click handler

                int row = i / tlpSeats.ColumnCount;
                int col = i % tlpSeats.ColumnCount;
                tlpSeats.Controls.Add(btn, col, row);
            }

            UpdateSummary();
        }

        private void CmbTimeSlot_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateSummary();
        }

        private void SeatButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not SeatInfo info) return;

            if (info.State == SeatState.Booked)
            {
                // Already booked / locked
                MessageBox.Show($"Vị trí {info.Index + 1} đã được đặt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Toggle selection
            if (info.State == SeatState.Empty)
            {
                info.State = SeatState.Selected;
                btn.BackColor = Color.LightGreen;
                selectedCount++;
            }
            else if (info.State == SeatState.Selected)
            {
                info.State = SeatState.Empty;
                btn.BackColor = Color.WhiteSmoke;
                selectedCount = Math.Max(0, selectedCount - 1);
            }

            btn.Tag = info;
            UpdateSummary();
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (selectedCount == 0)
            {
                MessageBox.Show("Chưa có vị trí nào được chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Mark selected seats as booked
            foreach (var btn in tlpSeats.Controls.OfType<Button>())
            {
                if (btn.Tag is SeatInfo info && info.State == SeatState.Selected)
                {
                    info.State = SeatState.Booked;
                    btn.Tag = info;
                    btn.BackColor = Color.IndianRed;
                    btn.ForeColor = Color.White;
                }
            }

            selectedCount = 0;
            UpdateSummary();
            MessageBox.Show("Đặt thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClearAll_Click(object? sender, EventArgs e)
        {
            foreach (var btn in tlpSeats.Controls.OfType<Button>())
            {
                if (btn.Tag is SeatInfo info && info.State == SeatState.Selected)
                {
                    info.State = SeatState.Empty;
                    btn.Tag = info;
                    btn.BackColor = Color.WhiteSmoke;
                    btn.ForeColor = Color.Black;
                }
            }

            selectedCount = 0;
            UpdateSummary();
        }

        private int CurrentRate => cmbTimeSlot.SelectedItem is TimeSlotItem tsi ? tsi.Price : 0;

        private void UpdateSummary()
        {
            lblCount.Text = $"Số vị trí đang chọn: {selectedCount}";
            lblTotal.Text = $"Tạm tính tiền: {selectedCount * CurrentRate:N0}đ";
        }
    }
}
