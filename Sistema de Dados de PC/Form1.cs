using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace CadastroProdutos
{
    // A estrutura deve ser idêntica às chaves do seu arquivo JSON
    public class Produto
    {
        public string codigo { get; set; }
        public string fabricante { get; set; }
        public double velocidade { get; set; }
        public int nucleos { get; set; }
        public int ram { get; set; }
        public string fabricante_processador { get; set; }
        public int ssd { get; set; }
    }

    public class MainForm : Form
    {
        private string jsonPath = "produtos_computador.json"; // Nome exato do seu arquivo
        private List<Produto> listaProdutos = new List<Produto>();
        
        private TextBox txtCodigo, txtVelocidade, txtSearch;
        private ComboBox comboFabricante, comboNucleos, comboRam, comboProcessador, comboSsd;
        private DataGridView dgvProdutos;

        public MainForm()
        {
            this.Text = "Cadastro de Produtos - Modo JSON";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            SetupUI();
            LoadDataFromFile();
            RefreshGrid();
        }

        private void SetupUI()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            this.Controls.Add(mainLayout);

            FlowLayoutPanel panelInputs = new FlowLayoutPanel { 
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(15), WrapContents = false 
            };
            mainLayout.Controls.Add(panelInputs, 0, 0);

            Font labelFont = new Font("Segoe UI", 9, FontStyle.Bold);

            // Inputs
            panelInputs.Controls.Add(new Label { Text = "Código do Produto:", Font = labelFont, AutoSize = true });
            txtCodigo = new TextBox { Width = 300 };
            panelInputs.Controls.Add(txtCodigo);

            panelInputs.Controls.Add(new Label { Text = "Fabricante:", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            comboFabricante = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboFabricante.Items.AddRange(new string[] { "Apple", "Microsoft", "Google", "Lenovo", "Sony", "Dell", "Samsung", "HP", "Asus" });
            panelInputs.Controls.Add(comboFabricante);

            panelInputs.Controls.Add(new Label { Text = "Velocidade (GHz):", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            txtVelocidade = new TextBox { Width = 300 };
            panelInputs.Controls.Add(txtVelocidade);

            panelInputs.Controls.Add(new Label { Text = "Núcleos:", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            comboNucleos = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboNucleos.Items.AddRange(new object[] { 2, 4, 6, 8, 10, 12, 16 });
            panelInputs.Controls.Add(comboNucleos);

            panelInputs.Controls.Add(new Label { Text = "Memória RAM (GB):", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            comboRam = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboRam.Items.AddRange(new object[] { 4, 8, 16, 32 });
            panelInputs.Controls.Add(comboRam);

            panelInputs.Controls.Add(new Label { Text = "Processador:", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            comboProcessador = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboProcessador.Items.AddRange(new string[] { "AMD", "Intel" });
            panelInputs.Controls.Add(comboProcessador);

            panelInputs.Controls.Add(new Label { Text = "SSD (GB):", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            comboSsd = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboSsd.Items.AddRange(new object[] { 128, 256, 512, 1024 });
            panelInputs.Controls.Add(comboSsd);

            // Botões
            panelInputs.Controls.Add(CreateButton("Adicionar", Color.FromArgb(76, 175, 80), (s, e) => SaveNewProduct()));
            panelInputs.Controls.Add(CreateButton("Carregar para Edição", Color.FromArgb(255, 165, 0), (s, e) => LoadToEdit()));
            panelInputs.Controls.Add(CreateButton("Salvar Alterações", Color.FromArgb(33, 150, 243), (s, e) => SaveEdit()));
            panelInputs.Controls.Add(CreateButton("Excluir", Color.FromArgb(244, 67, 54), (s, e) => DeleteProduct()));

            panelInputs.Controls.Add(new Label { Text = "Buscar:", Font = labelFont, Margin = new Padding(0, 20, 0, 0) });
            txtSearch = new TextBox { Width = 300 };
            panelInputs.Controls.Add(txtSearch);
            panelInputs.Controls.Add(CreateButton("Filtrar", Color.FromArgb(33, 150, 243), (s, e) => SearchProducts()));

            // Grid Configurado para exibir as propriedades minúsculas do JSON
            dgvProdutos = new DataGridView { 
                Dock = DockStyle.Fill, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            mainLayout.Controls.Add(dgvProdutos, 1, 0);
        }

        private Button CreateButton(string text, Color color, EventHandler onClick)
        {
            var btn = new Button {
                Text = text, BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Width = 300, Height = 35,
                Margin = new Padding(0, 10, 0, 0), Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btn.Click += onClick;
            return btn;
        }

        private void LoadDataFromFile()
        {
            if (File.Exists(jsonPath))
            {
                string jsonString = File.ReadAllText(jsonPath);
                // Desserializa garantindo que mapeie as chaves minúsculas
                listaProdutos = JsonSerializer.Deserialize<List<Produto>>(jsonString) ?? new List<Produto>();
            }
        }

        private void RefreshGrid(List<Produto> displayList = null)
        {
            // Vincula diretamente a lista para evitar discrepâncias de nomes de colunas
            dgvProdutos.DataSource = null;
            dgvProdutos.DataSource = displayList ?? listaProdutos;

            if (dgvProdutos.Columns.Count > 0)
            {
                dgvProdutos.Columns["codigo"].HeaderText = "Código";
                dgvProdutos.Columns["fabricante"].HeaderText = "Fabricante";
                dgvProdutos.Columns["velocidade"].HeaderText = "Velocidade (GHz)";
                dgvProdutos.Columns["nucleos"].HeaderText = "Núcleos";
                dgvProdutos.Columns["ram"].HeaderText = "RAM (GB)";
                dgvProdutos.Columns["fabricante_processador"].HeaderText = "Processador";
                dgvProdutos.Columns["ssd"].HeaderText = "SSD (GB)";
            }
        }

        private void SaveNewProduct()
        {
            double.TryParse(txtVelocidade.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double vel);
            
            listaProdutos.Add(new Produto {
                codigo = txtCodigo.Text,
                fabricante = comboFabricante.Text,
                velocidade = vel,
                nucleos = int.Parse(comboNucleos.Text ?? "0"),
                ram = int.Parse(comboRam.Text ?? "0"),
                fabricante_processador = comboProcessador.Text,
                ssd = int.Parse(comboSsd.Text ?? "0")
            });

            File.WriteAllText(jsonPath, JsonSerializer.Serialize(listaProdutos, new JsonSerializerOptions { WriteIndented = true }));
            RefreshGrid();
        }

        private void LoadToEdit()
        {
            if (dgvProdutos.SelectedRows.Count > 0)
            {
                var p = (Produto)dgvProdutos.SelectedRows[0].DataBoundItem;
                txtCodigo.Text = p.codigo;
                comboFabricante.Text = p.fabricante;
                txtVelocidade.Text = p.velocidade.ToString();
                comboNucleos.Text = p.nucleos.ToString();
                comboRam.Text = p.ram.ToString();
                comboProcessador.Text = p.fabricante_processador;
                comboSsd.Text = p.ssd.ToString();
            }
        }

        private void SaveEdit()
        {
            if (dgvProdutos.SelectedRows.Count > 0)
            {
                var pOriginal = (Produto)dgvProdutos.SelectedRows[0].DataBoundItem;
                double.TryParse(txtVelocidade.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double vel);

                pOriginal.codigo = txtCodigo.Text;
                pOriginal.fabricante = comboFabricante.Text;
                pOriginal.velocidade = vel;
                pOriginal.nucleos = int.Parse(comboNucleos.Text);
                pOriginal.ram = int.Parse(comboRam.Text);
                pOriginal.fabricante_processador = comboProcessador.Text;
                pOriginal.ssd = int.Parse(comboSsd.Text);

                File.WriteAllText(jsonPath, JsonSerializer.Serialize(listaProdutos, new JsonSerializerOptions { WriteIndented = true }));
                RefreshGrid();
            }
        }

        private void DeleteProduct()
        {
            if (dgvProdutos.SelectedRows.Count > 0)
            {
                var p = (Produto)dgvProdutos.SelectedRows[0].DataBoundItem;
                listaProdutos.Remove(p);
                File.WriteAllText(jsonPath, JsonSerializer.Serialize(listaProdutos, new JsonSerializerOptions { WriteIndented = true }));
                RefreshGrid();
            }
        }

        private void SearchProducts()
        {
            string termo = txtSearch.Text.ToLower();
            var filtrados = listaProdutos.Where(p => 
                p.fabricante.ToLower().Contains(termo) || 
                p.codigo.ToLower().Contains(termo)
            ).ToList();
            RefreshGrid(filtrados);
        }
    }
}