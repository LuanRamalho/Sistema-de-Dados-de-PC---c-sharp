using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace CadastroProdutos
{
    public class MainForm : Form
    {
        private string connString = "Data Source=produtos_computador.db;Version=3;";
        
        // Componentes da Interface
        private TextBox txtCodigo, txtVelocidade, txtSearch;
        private ComboBox comboFabricante, comboNucleos, comboRam, comboProcessador, comboSsd;
        private DataGridView dgvProdutos;
        private int? selectedProductId = null;

        public MainForm()
        {
            // Configurações da Janela
            this.Text = "Cadastro de Produtos de Computador";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            InitDatabase();
            SetupUI();
            LoadProducts();
        }

        private void InitDatabase()
        {
            using (var conn = new SQLiteConnection(connString))
            {
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS produtos (
                                id INTEGER PRIMARY KEY AUTOINCREMENT,
                                codigo TEXT,
                                fabricante TEXT,
                                velocidade REAL,
                                nucleos INTEGER,
                                ram INTEGER,
                                fabricante_processador TEXT,
                                ssd INTEGER)";
                var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        private void SetupUI()
        {
            // Layout Principal
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            this.Controls.Add(mainLayout);

            // Painel de Inputs (Esquerda)
            FlowLayoutPanel panelInputs = new FlowLayoutPanel { 
                Dock = DockStyle.Fill, 
                FlowDirection = FlowDirection.TopDown, 
                Padding = new Padding(15),
                WrapContents = false 
            };
            panelInputs.BackColor = Color.FromArgb(240, 240, 240);
            mainLayout.Controls.Add(panelInputs, 0, 0);

            // Estilo de labels e inputs
            Font labelFont = new Font("Segoe UI", 9, FontStyle.Bold);
            Size inputSize = new Size(300, 25);

            // Criando Campos
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
            comboNucleos.Items.AddRange(new object[] { 4, 6, 8, 10, 12, 16 });
            panelInputs.Controls.Add(comboNucleos);

            panelInputs.Controls.Add(new Label { Text = "Memória RAM (GB):", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            comboRam = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboRam.Items.AddRange(new object[] { 8, 16, 32 });
            panelInputs.Controls.Add(comboRam);

            panelInputs.Controls.Add(new Label { Text = "Processador:", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            comboProcessador = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboProcessador.Items.AddRange(new string[] { "AMD", "Intel" });
            panelInputs.Controls.Add(comboProcessador);

            panelInputs.Controls.Add(new Label { Text = "SSD (GB):", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            comboSsd = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            comboSsd.Items.AddRange(new object[] { 256, 512, 1024 });
            panelInputs.Controls.Add(comboSsd);

            // Botões Coloridos
            Button btnAdd = CreateButton("Adicionar", Color.FromArgb(76, 175, 80), Color.White);
            btnAdd.Click += (s, e) => SaveProduct();
            panelInputs.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("Carregar para Edição", Color.FromArgb(255, 165, 0), Color.White);
            btnEdit.Click += (s, e) => LoadToEdit();
            panelInputs.Controls.Add(btnEdit);

            Button btnSaveEdit = CreateButton("Salvar Alterações", Color.FromArgb(33, 150, 243), Color.White);
            btnSaveEdit.Click += (s, e) => SaveEdit();
            panelInputs.Controls.Add(btnSaveEdit);

            Button btnDelete = CreateButton("Excluir", Color.FromArgb(244, 67, 54), Color.White);
            btnDelete.Click += (s, e) => DeleteProduct();
            panelInputs.Controls.Add(btnDelete);

            // Busca
            panelInputs.Controls.Add(new Label { Text = "Buscar:", Font = labelFont, Margin = new Padding(0, 20, 0, 0) });
            txtSearch = new TextBox { Width = 300 };
            panelInputs.Controls.Add(txtSearch);
            Button btnSearch = CreateButton("Filtrar", Color.FromArgb(33, 150, 243), Color.White);
            btnSearch.Click += (s, e) => SearchProducts();
            panelInputs.Controls.Add(btnSearch);

            // Tabela (Direita)
            dgvProdutos = new DataGridView { 
                Dock = DockStyle.Fill, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            dgvProdutos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            mainLayout.Controls.Add(dgvProdutos, 1, 0);
        }

        private Button CreateButton(string text, Color backColor, Color foreColor)
        {
            return new Button {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Width = 300,
                Height = 35,
                Margin = new Padding(0, 10, 0, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
        }

        // Lógica CRUD
        private void SaveProduct()
        {
            using (var conn = new SQLiteConnection(connString))
            {
                conn.Open();
                string sql = "INSERT INTO produtos (codigo, fabricante, velocidade, nucleos, ram, fabricante_processador, ssd) VALUES (?, ?, ?, ?, ?, ?, ?)";
                var cmd = new SQLiteCommand(sql, conn);

                // Trata a velocidade para aceitar ponto ou vírgula corretamente
                string velTexto = txtVelocidade.Text.Replace(',', '.');
                double.TryParse(velTexto, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double velFinal);

                cmd.Parameters.AddWithValue("1", txtCodigo.Text);
                cmd.Parameters.AddWithValue("2", comboFabricante.Text);
                cmd.Parameters.AddWithValue("3", velFinal);
                cmd.Parameters.AddWithValue("4", comboNucleos.Text);
                cmd.Parameters.AddWithValue("5", comboRam.Text);
                cmd.Parameters.AddWithValue("6", comboProcessador.Text);
                cmd.Parameters.AddWithValue("7", comboSsd.Text);
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Produto salvo com sucesso!");
            LoadProducts();
        }

        private void LoadProducts()
        {
            using (var conn = new SQLiteConnection(connString))
            {
                conn.Open();
                string sql = "SELECT * FROM produtos";
                SQLiteDataAdapter da = new SQLiteDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Adiciona uma coluna temporária para o índice visual (1, 2, 3...)
                dt.Columns.Add("Nº", typeof(int)).SetOrdinal(0);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["Nº"] = i + 1;
                }

                dgvProdutos.DataSource = dt;
                dgvProdutos.Columns["velocidade"].DefaultCellStyle.Format = "N1"; // Exibe sempre 1 casa decimal

                // Oculta o ID real (índice 1 agora, pois o Nº é o 0) 
                // para que o usuário não veja os números "pulando" do banco
                if (dgvProdutos.Columns.Contains("id"))
                {
                    dgvProdutos.Columns["id"].Visible = false;
                }
                
                // Ajusta os nomes das colunas para ficarem bonitos
                dgvProdutos.Columns["codigo"].HeaderText = "Código";
                dgvProdutos.Columns["fabricante"].HeaderText = "Fabricante";
                dgvProdutos.Columns["velocidade"].HeaderText = "Velocidade (GHz)";
                dgvProdutos.Columns["nucleos"].HeaderText = "Núcleos";
                dgvProdutos.Columns["ram"].HeaderText = "RAM (GB)";
                dgvProdutos.Columns["fabricante_processador"].HeaderText = "Processador";
                dgvProdutos.Columns["ssd"].HeaderText = "SSD (GB)";
            }
        }

        private void LoadToEdit()
        {
            if (dgvProdutos.SelectedRows.Count > 0)
            {
                var row = dgvProdutos.SelectedRows[0];
                selectedProductId = Convert.ToInt32(row.Cells["id"].Value);
                txtCodigo.Text = row.Cells["codigo"].Value.ToString();
                comboFabricante.Text = row.Cells["fabricante"].Value.ToString();
                txtVelocidade.Text = row.Cells["velocidade"].Value.ToString();
                comboNucleos.Text = row.Cells["nucleos"].Value.ToString();
                comboRam.Text = row.Cells["ram"].Value.ToString();
                comboProcessador.Text = row.Cells["fabricante_processador"].Value.ToString();
                comboSsd.Text = row.Cells["ssd"].Value.ToString();
            }
            else
            {
                MessageBox.Show("Selecione um produto na tabela primeiro.");
            }
        }

        private void SaveEdit()
        {
            if (selectedProductId == null) return;

            using (var conn = new SQLiteConnection(connString))
            {
                conn.Open();
                string sql = @"UPDATE produtos SET codigo=?, fabricante=?, velocidade=?, nucleos=?, ram=?, fabricante_processador=?, ssd=? WHERE id=?";
                var cmd = new SQLiteCommand(sql, conn);

                // Trata a velocidade para aceitar ponto ou vírgula corretamente
                string velTexto = txtVelocidade.Text.Replace(',', '.');
                double.TryParse(velTexto, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double velFinal);

                cmd.Parameters.AddWithValue("1", txtCodigo.Text);
                cmd.Parameters.AddWithValue("2", comboFabricante.Text);
                cmd.Parameters.AddWithValue("3", velFinal);
                cmd.Parameters.AddWithValue("4", comboNucleos.Text);
                cmd.Parameters.AddWithValue("5", comboRam.Text);
                cmd.Parameters.AddWithValue("6", comboProcessador.Text);
                cmd.Parameters.AddWithValue("7", comboSsd.Text);
                cmd.Parameters.AddWithValue("8", selectedProductId);
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Atualizado com sucesso!");
            selectedProductId = null;
            LoadProducts();
        }

        private void DeleteProduct()
        {
            if (dgvProdutos.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvProdutos.SelectedRows[0].Cells["id"].Value);
                using (var conn = new SQLiteConnection(connString))
                {
                    conn.Open();
                    var cmd = new SQLiteCommand("DELETE FROM produtos WHERE id = ?", conn);
                    cmd.Parameters.AddWithValue("1", id);
                    cmd.ExecuteNonQuery();
                }
                LoadProducts();
                MessageBox.Show("Excluído com sucesso!");
            }
        }

        private void SearchProducts()
        {
            using (var conn = new SQLiteConnection(connString))
            {
                conn.Open();
                string sql = "SELECT * FROM produtos WHERE fabricante LIKE @p OR fabricante_processador LIKE @p OR codigo LIKE @p";
                SQLiteDataAdapter da = new SQLiteDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@p", "%" + txtSearch.Text + "%");
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvProdutos.DataSource = dt;
            }
        }
    }
}