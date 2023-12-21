using Backprop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PantojaBackPropag
{
    public partial class Form1 : Form
    {
        private NeuralNet NN;
        private int inp;
        private int hid;
        private int oup = 1;
        private int NTrain;

        public Form1()
        {
            InitializeComponent();
            InitializeStart();
            InitializeDataGridView();
        }

        private void InitializeStart()
        {
            gbTraining.Enabled = false;
            dgvInputOutput.Enabled = false;

            inp = 4;
            hid = (int)numUDNeuron.Value;

            btnTest.Enabled = false;
            btnClear.Enabled = false;

            btnSave.Enabled = false;

            NTrain = (int)numUDEpoch.Value;
        }
        
        private void InitializeDataGridView()
        {
            dgvInputOutput.Columns["I0"].Name = "Input0";
        }

        private void TrainModel()
        {
            if (inp == 4)
            {
                List<Tuple<double[], double[]>> trainingData = rbInputAND4.Checked ? TrainingData.GetTrainingDataB() : TrainingData.GetTrainingDataD();

                for (int i = 0; i < NTrain; i++)
                {
                    foreach (var data in trainingData)
                    {
                        NN.setInputs(0, data.Item1[0]);
                        NN.setInputs(1, data.Item1[1]);
                        NN.setInputs(2, data.Item1[2]);
                        NN.setInputs(3, data.Item1[3]);
                        NN.setDesiredOutput(0, data.Item2[0]);
                        NN.learn();
                    }
                }
            }
        }
        private void numUDNeuron_ValueChanged(object sender, EventArgs e)
        {
            hid = (int)numUDNeuron.Value;
        }

        private void rbInputAND4_CheckedChanged(object sender, EventArgs e)
        {
            if (rbInputAND4.Checked)
            {
                dgvInputOutput.Columns["Input3"].Visible = true;
                dgvInputOutput.Columns["Input2"].Visible = true;
                inp = 4;
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            NTrain = (int)numUDEpoch.Value;
        }

        private void btnTrain_Click(object sender, EventArgs e)
        {
            TrainModel();
            btnSave.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Neural Network Weights";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    try
                    {
                        NN.saveWeights(filePath);
                        MessageBox.Show("Weights saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving weights: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.Title = "Load Neural Network Weights";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    try
                    {
                        NN.loadWeights(filePath);
                        MessageBox.Show("Weights loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnSave.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading weights: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
      
        private void dgvInputOutput_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string columnName = dgvInputOutput.Columns[e.ColumnIndex].Name;

            if (!columnName.Equals("Output", StringComparison.OrdinalIgnoreCase))
            {
                if (e.FormattedValue.ToString() != "0" && e.FormattedValue.ToString() != "1" && e.FormattedValue.ToString() != "")
                {
                    e.Cancel = true;
                    dgvInputOutput.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = null;
                    MessageBox.Show("Please enter only 0 or 1.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvInputOutput.Rows)
            {
                int inputCount = (rbInputAND4.Checked) ? 2 : 4;

                string in0 = !string.IsNullOrEmpty(row.Cells["Input0"].Value?.ToString()) ? row.Cells["Input0"].Value.ToString() : "-";
                string in1 = !string.IsNullOrEmpty(row.Cells["Input1"].Value?.ToString()) ? row.Cells["Input1"].Value.ToString() : "-";
                string in2 = !rbInputAND4.Checked ? (!string.IsNullOrEmpty(row.Cells["Input2"].Value?.ToString()) ? row.Cells["Input2"].Value.ToString() : "-") : "-";
                string in3 = !rbInputAND4.Checked ? (!string.IsNullOrEmpty(row.Cells["Input3"].Value?.ToString()) ? row.Cells["Input3"].Value.ToString() : "-") : "-";

                bool isValidRow = (inputCount == 2)
                            ? (in0 != "-" && in1 != "-")
                            : (in0 != "-" && in1 != "-" && in2 != "-" && in3 != "-");

                if (isValidRow)
                {
                    double[] inputs = new double[inputCount];

                    for (int i = 0; i < inputCount; i++)
                    {
                        string columnName = $"Input{i}";
                        inputs[i] = Convert.ToDouble(row.Cells[columnName].Value);
                    }

                    for (int i = 0; i < inputCount; i++)
                    {
                        NN.setInputs(i, inputs[i]);
                    }

                    NN.run();

                    double outputValue = NN.getOuputData(0);
                    row.Cells["Output"].Value = outputValue;
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            gbTraining.Enabled = true;

            lblHN.Enabled = false;
            numUDNeuron.Enabled = false;
            dgvInputOutput.Enabled = true;
            btnTest.Enabled = true;
            btnClear.Enabled = true;


            btnCreate.Enabled = false;

            NN = new NeuralNet(inp, hid, oup);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            gbTraining.Enabled = false;

            lblHN.Enabled = true;
            numUDNeuron.Enabled = true;
            dgvInputOutput.Enabled = false;
            btnTest.Enabled = false;
            btnClear.Enabled = false;


            btnCreate.Enabled = true;
            btnSave.Enabled = false;

            NN = null;

            ClearTable();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearTable();
        }

        private void ClearTable()
        {
            if (chkKeepInputs.Checked)
            {
                ClearColumn("Output");
            }
            else
            {
                dgvInputOutput.Rows.Clear();
                dgvInputOutput.Refresh();
            }
        }

        private void ClearColumn(string columnName)
        {
            if (dgvInputOutput.Columns.Contains(columnName))
            {
                foreach (DataGridViewRow row in dgvInputOutput.Rows)
                {
                    row.Cells[columnName].Value = null;
                }
            }
            else
            {
                MessageBox.Show($"Column '{columnName}' not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // END INPUT OUTPUT
    }
}
