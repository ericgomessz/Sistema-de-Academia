using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NexusFit.Models
{
    public abstract class Usuario
    {
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private readonly string _cpf;
        private string _senhaHash = string.Empty;

        protected Usuario(string nome, string email, string cpf, string senhaPlana)
        {
            Nome = nome;
            Email = email;
            _cpf = ValidarEFormatarCpf(cpf);
            DefinirSenha(senhaPlana);
        }

        public string Nome
        {
            get => _nome;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 2)
                    throw new RegraNegocioException("Nome deve ter pelo menos 2 caracteres.");
                _nome = value.Trim();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || !value.Contains('@') || !value.Contains('.'))
                    throw new RegraNegocioException("E-mail inválido.");
                _email = value.Trim().ToLowerInvariant();
            }
        }

        public string Cpf => _cpf;

        public void DefinirSenha(string senhaPlana)
        {
            if (string.IsNullOrWhiteSpace(senhaPlana) || senhaPlana.Length < 6)
                throw new RegraNegocioException("A senha deve ter no mínimo 6 caracteres.");

            _senhaHash = GerarHash(senhaPlana);
        }

        public bool Autenticar(string senhaPlana)
        {
            if (string.IsNullOrEmpty(senhaPlana))
                return false;

            return GerarHash(senhaPlana) == _senhaHash;
        }

        protected static string GerarHash(string texto)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }

        private static string ValidarEFormatarCpf(string cpf)
        {
            string digitos = new string((cpf ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digitos.Length != 11 || digitos.Distinct().Count() == 1)
                throw new RegraNegocioException("CPF inválido: deve conter 11 dígitos válidos.");

            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = digitos[..9];
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (tempCpf[i] - '0') * multiplicadores1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCpf += digito1;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (tempCpf[i] - '0') * multiplicadores2[i];

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            if (digitos != tempCpf + digito2)
                throw new RegraNegocioException("CPF inválido: dígitos verificadores não conferem.");

            return Convert.ToUInt64(digitos).ToString(@"000\.000\.000\-00");
        }

        public abstract void ExibirPainel(NexusFitSystem sistema);
    }

    public class Membro : Usuario
    {
        private int _mesesFidelidade;
        private int _convitesAmigosRestantesMes;
        private int _aulasLivresRestantesMes;
        private readonly List<Pagamento> _pagamentos = new();
        private readonly List<Treino> _treinos = new();
        private readonly List<AvaliacaoFisica> _avaliacoes = new();

        public Membro(string nome, string email, string cpf, string senhaPlana, string matricula)
            : base(nome, email, cpf, senhaPlana)
        {
            if (string.IsNullOrWhiteSpace(matricula))
                throw new RegraNegocioException("Matrícula do membro é obrigatória.");

            Matricula = matricula.Trim().ToUpperInvariant();
            Status = StatusMatricula.Inativa;
            DataAdesao = DateTime.Today;
            _mesesFidelidade = 0;
            ResetarSaldosMensais();
        }

        public string Matricula { get; }
        public StatusMatricula Status { get; private set; }
        public DateTime DataAdesao { get; }
        public Plano? PlanoAtivo { get; private set; }

        public int MesesFidelidade
        {
            get => _mesesFidelidade;
            private set
            {
                if (value < 0)
                    throw new RegraNegocioException("Meses de fidelidade não podem ser negativos.");
                _mesesFidelidade = value;
            }
        }

        public int ConvitesAmigosRestantesMes => _convitesAmigosRestantesMes;
        public int AulasLivresRestantesMes => _aulasLivresRestantesMes;

        public IReadOnlyList<Pagamento> Pagamentos => _pagamentos.AsReadOnly();
        public IReadOnlyList<Treino> Treinos => _treinos.AsReadOnly();
        public IReadOnlyList<AvaliacaoFisica> Avaliacoes => _avaliacoes.AsReadOnly();

        public bool PossuiPendenciaFinanceira => _pagamentos.Any(p => !p.Pago);

        private void ResetarSaldosMensais()
        {
            CategoriaPlano? cat = PlanoAtivo?.Categoria;
            _convitesAmigosRestantesMes = cat == CategoriaPlano.Diamante ? RegrasNegocioConfig.LimiteConvitesDiamanteMes : 0;
            _aulasLivresRestantesMes = cat == CategoriaPlano.Prata ? RegrasNegocioConfig.LimiteAulaLivrePrataMes : 0;
        }

        public void ContratarPlano(Plano novoPlano)
        {
            if (novoPlano == null) throw new RegraNegocioException("Plano não pode ser nulo.");
            if (PlanoAtivo != null && Status == StatusMatricula.Ativa)
                throw new RegraNegocioException("Membro já possui um plano ativo. Utilize 'AlterarPlano' ou 'RenovarPlano'.");

            PlanoAtivo = novoPlano;
            Status = StatusMatricula.Ativa;
            ResetarSaldosMensais();
            GerarFaturaMensal();
        }

        public void AlterarPlano(Plano novoPlano)
        {
            if (novoPlano == null) throw new RegraNegocioException("Novo plano não pode ser nulo.");
            if (Status == StatusMatricula.Cancelada) throw new RegraNegocioException("Matrícula cancelada não pode alterar plano.");

            PlanoAtivo = novoPlano;
            Status = StatusMatricula.Ativa;
            ResetarSaldosMensais();
            GerarFaturaMensal();
        }

        public void RenovarPlano()
        {
            if (PlanoAtivo == null) throw new RegraNegocioException("Membro não possui plano para renovar.");
            if (Status == StatusMatricula.Cancelada) throw new RegraNegocioException("Matrícula cancelada não pode ser renovada.");

            MesesFidelidade += PlanoAtivo.DuracaoMeses;
            Status = StatusMatricula.Ativa;
            ResetarSaldosMensais();
            GerarFaturaMensal();
        }

        public void SolicitarTrancamento()
        {
            if (PlanoAtivo == null) throw new RegraNegocioException("Nenhum plano ativo para trancar.");
            if (PossuiPendenciaFinanceira) throw new RegraNegocioException("Não é possível trancar matrícula com pendência financeira.");
            if (!PlanoAtivo.PermiteTrancamento())
                throw new RegraNegocioException($"O plano {PlanoAtivo.DescricaoDuracao} não permite trancamento.");

            Status = StatusMatricula.Trancada;
        }

        public void CancelarPlano()
        {
            if (Status == StatusMatricula.Cancelada) throw new RegraNegocioException("A matrícula já está cancelada.");

            PlanoAtivo = null;
            Status = StatusMatricula.Cancelada;
            _convitesAmigosRestantesMes = 0;
            _aulasLivresRestantesMes = 0;
        }

        internal void DefinirStatusAdministrativo(StatusMatricula novoStatus)
        {
            Status = novoStatus;
            if (novoStatus == StatusMatricula.Cancelada) PlanoAtivo = null;
        }

        public void UsarConviteAmigo()
        {
            ValidarAcessoBasico();
            if (PlanoAtivo?.Categoria != CategoriaPlano.Diamante)
                throw new RegraNegocioException("Apenas a categoria Diamante possui convites de amigos.");
            if (_convitesAmigosRestantesMes <= 0)
                throw new RegraNegocioException("Limite de convites de amigos no mês esgotado.");

            _convitesAmigosRestantesMes--;
        }

        public void UsarAulaLivre()
        {
            ValidarAcessoBasico();
            if (PlanoAtivo?.Categoria != CategoriaPlano.Prata)
                throw new RegraNegocioException("Aulas avulsas mensais aplicam-se ao plano Prata.");
            if (_aulasLivresRestantesMes <= 0)
                throw new RegraNegocioException("Limite de aula avulsa do mês já utilizado.");

            _aulasLivresRestantesMes--;
        }

        public void ValidarAcessoBasico()
        {
            if (Status != StatusMatricula.Ativa || PlanoAtivo == null)
                throw new RegraNegocioException($"Acesso negado: matrícula está {Status}.");
            if (PossuiPendenciaFinanceira)
                throw new RegraNegocioException("Acesso negado: existem pagamentos pendentes.");
        }

        public void GerarFaturaMensal()
        {
            if (PlanoAtivo == null) throw new RegraNegocioException("Não é possível gerar fatura sem plano ativo.");
            decimal valor = PlanoAtivo.CalcularMensalidade(MesesFidelidade);
            _pagamentos.Add(new Pagamento(valor, DateTime.Today.AddDays(30)));
        }

        internal void AdicionarTreino(Treino treino) => _treinos.Add(treino);
        internal void AdicionarAvaliacao(AvaliacaoFisica avaliacao) => _avaliacoes.Add(avaliacao);

        public void ConsultarPagamentos()
        {
            Console.WriteLine($"\n--- Histórico de Pagamentos ({Nome}) ---");
            if (_pagamentos.Count == 0) { Console.WriteLine("Nenhuma fatura registrada."); return; }

            foreach (var p in _pagamentos)
                Console.WriteLine($"Vencimento: {p.Vencimento:dd/MM/yyyy} | Valor: R$ {p.Valor:F2} | Status: {(p.Pago ? "PAGO" : "PENDENTE")}");
        }

        public void ConsultarEvolucao()
        {
            Console.WriteLine($"\n--- Evolução Física ({Nome}) ---");
            if (_avaliacoes.Count == 0) { Console.WriteLine("Nenhuma avaliação física registrada."); return; }

            foreach (var av in _avaliacoes.OrderBy(a => a.Data))
                Console.WriteLine($"{av.Data:dd/MM/yyyy} | Peso: {av.Peso:F1} kg | {av.Medidas}");
        }

        public void ExibirTreinos()
        {
            Console.WriteLine($"\n--- Treinos de {Nome} ---");
            if (_treinos.Count == 0) { Console.WriteLine("Nenhum treino cadastrado para você ainda."); return; }

            foreach (var t in _treinos)
            {
                Console.WriteLine($"\nTreino: {t.Nome} (Criado em {t.DataCriacao:dd/MM/yyyy} por {t.InstrutorResponsavel.Nome})");
                foreach (var ex in t.Exercicios)
                    Console.WriteLine($"  • {ex.Descrever()}");
            }
        }

        public override void ExibirPainel(NexusFitSystem sistema)
        {
            while (true)
            {
                string planoTexto = PlanoAtivo != null
                    ? $"{PlanoAtivo.DescricaoDuracao} - {PlanoAtivo.Categoria} (R$ {PlanoAtivo.CalcularMensalidade(MesesFidelidade):F2}/mês)"
                    : "Nenhum";

                Console.WriteLine("\n==========================================");
                Console.WriteLine($"PAINEL DO MEMBRO — {Nome} ({Matricula})");
                Console.WriteLine($"Status: {Status} | Plano: {planoTexto} | Fidelidade: {MesesFidelidade} meses");
                Console.WriteLine($"Pendência financeira: {(PossuiPendenciaFinanceira ? "SIM" : "NÃO")}");
                if (PlanoAtivo?.Categoria == CategoriaPlano.Diamante)
                    Console.WriteLine($"Convites amigos restantes no mês: {ConvitesAmigosRestantesMes}");
                if (PlanoAtivo?.Categoria == CategoriaPlano.Prata)
                    Console.WriteLine($"Aulas livres restantes no mês: {AulasLivresRestantesMes}");
                Console.WriteLine("==========================================");
                Console.WriteLine("1. Contratar / Alterar Plano");
                Console.WriteLine("2. Renovar Plano");
                Console.WriteLine("3. Solicitar Trancamento");
                Console.WriteLine("4. Cancelar Plano");
                Console.WriteLine("5. Consultar Pagamentos e Status");
                Console.WriteLine("6. Ver Meus Treinos");
                Console.WriteLine("7. Consultar Evolução Física");
                Console.WriteLine("8. Usar Convite de Amigo (Diamante)");
                Console.WriteLine("9. Usar Aula Livre do Mês (Prata)");
                Console.WriteLine("0. Sair / Logout");
                Console.Write("Escolha: ");

                string? opc = Console.ReadLine()?.Trim();
                if (opc == "0") break;

                try
                {
                    switch (opc)
                    {
                        case "1":
                            Plano novoPlano = sistema.LerEscolhaPlano();
                            if (PlanoAtivo == null || Status != StatusMatricula.Ativa) ContratarPlano(novoPlano);
                            else AlterarPlano(novoPlano);
                            Console.WriteLine("Plano atualizado com sucesso!");
                            break;
                        case "2":
                            RenovarPlano();
                            Console.WriteLine("Plano renovado com sucesso!");
                            break;
                        case "3":
                            SolicitarTrancamento();
                            Console.WriteLine("Trancamento realizado com sucesso!");
                            break;
                        case "4":
                            CancelarPlano();
                            Console.WriteLine("Plano cancelado.");
                            break;
                        case "5":
                            ConsultarPagamentos();
                            Console.WriteLine($"Status atual da matrícula: {Status}");
                            break;
                        case "6":
                            ExibirTreinos();
                            break;
                        case "7":
                            ConsultarEvolucao();
                            break;
                        case "8":
                            UsarConviteAmigo();
                            Console.WriteLine("Convite de amigo registrado!");
                            break;
                        case "9":
                            UsarAulaLivre();
                            Console.WriteLine("Aula livre utilizada!");
                            break;
                        default:
                            Console.WriteLine("Opção inválida.");
                            break;
                    }
                }
                catch (Exception ex) when (ex is RegraNegocioException or FormatException or ArgumentOutOfRangeException)
                {
                    Console.WriteLine($"[ERRO] {ex.Message}");
                }
            }
        }
    }

    public class Funcionario : Usuario
    {
        private decimal _salario;

        public Funcionario(string nome, string email, string cpf, string senhaPlana, string cargo, decimal salario)
            : base(nome, email, cpf, senhaPlana)
        {
            if (string.IsNullOrWhiteSpace(cargo))
                throw new RegraNegocioException("Cargo é obrigatório.");

            Cargo = cargo.Trim();
            Salario = salario;
        }

        public string Cargo { get; }

        public decimal Salario
        {
            get => _salario;
            set
            {
                if (value <= 0) throw new RegraNegocioException("Salário deve ser maior que zero.");
                _salario = value;
            }
        }

        public void ConsultarPendencias(IEnumerable<Membro> membros)
        {
            Console.WriteLine("\n--- Consulta de Pendências (Ativo / Inativo) ---");
            foreach (var m in membros)
            {
                string estado = (m.Status == StatusMatricula.Ativa && !m.PossuiPendenciaFinanceira) ? "ATIVO" : "INATIVO";
                Console.WriteLine($"Matrícula: {m.Matricula} | Nome: {m.Nome} | CPF: {m.Cpf} | Situação: {estado} (Matrícula: {m.Status})");
            }
        }

        public void EfetuarCobranca(Membro membro)
        {
            if (membro == null) throw new RegraNegocioException("Membro inválido.");

            var pendente = membro.Pagamentos.FirstOrDefault(p => !p.Pago);
            if (pendente == null) throw new RegraNegocioException("Membro não possui cobranças pendentes.");

            pendente.MarcarComoPago();
            if (membro.Status == StatusMatricula.Inativa && membro.PlanoAtivo != null)
                membro.DefinirStatusAdministrativo(StatusMatricula.Ativa);
        }

        public void CancelarMatricula(Membro membro)
        {
            if (membro == null) throw new RegraNegocioException("Membro inválido.");
            membro.CancelarPlano();
        }

        public override void ExibirPainel(NexusFitSystem sistema)
        {
            while (true)
            {
                Console.WriteLine("\n==========================================");
                Console.WriteLine($"PAINEL DO FUNCIONÁRIO — {Nome} ({Cargo})");
                Console.WriteLine("==========================================");
                Console.WriteLine("1. Cadastrar Novo Membro");
                Console.WriteLine("2. Consultar Dados Cadastrais dos Membros");
                Console.WriteLine("3. Consultar Pendências (Ativo / Inativo)");
                Console.WriteLine("4. Efetuar Cobrança de Membro");
                Console.WriteLine("5. Cancelar Matrícula de Membro");
                Console.WriteLine("0. Sair / Logout");
                Console.Write("Escolha: ");

                string? opc = Console.ReadLine()?.Trim();
                if (opc == "0") break;

                try
                {
                    switch (opc)
                    {
                        case "1": sistema.CadastrarMembroViaConsole(); break;
                        case "2": sistema.ListarDadosMembros(); break;
                        case "3": ConsultarPendencias(sistema.ObterMembros()); break;
                        case "4":
                            var membroCobrar = sistema.BuscarMembroPorMatriculaViaConsole();
                            EfetuarCobranca(membroCobrar);
                            Console.WriteLine("Cobrança efetuada e status atualizado!");
                            break;
                        case "5":
                            var membroCancelar = sistema.BuscarMembroPorMatriculaViaConsole();
                            CancelarMatricula(membroCancelar);
                            Console.WriteLine("Matrícula cancelada com sucesso!");
                            break;
                        default: Console.WriteLine("Opção inválida."); break;
                    }
                }
                catch (Exception ex) when (ex is RegraNegocioException or FormatException or ArgumentOutOfRangeException)
                {
                    Console.WriteLine($"[ERRO] {ex.Message}");
                }
            }
        }
    }

    public class Instrutor : Funcionario
    {
        public Instrutor(string nome, string email, string cpf, string senhaPlana, decimal salario)
            : base(nome, email, cpf, senhaPlana, "Instrutor", salario) { }

        public Treino MontarTreino(Membro membro, string nomeTreino, IEnumerable<Exercicio> exercicios)
        {
            if (membro == null) throw new RegraNegocioException("Membro inválido.");

            var treino = new Treino(nomeTreino, this);
            foreach (var ex in exercicios)
                treino.AdicionarExercicio(ex);

            membro.AdicionarTreino(treino);
            return treino;
        }

        public AvaliacaoFisica RegistrarAvaliacao(Membro membro, DateTime data, decimal peso, MedidasCorporais medidas)
        {
            if (membro == null) throw new RegraNegocioException("Membro inválido.");

            var avaliacao = new AvaliacaoFisica(data, peso, medidas, this);
            membro.AdicionarAvaliacao(avaliacao);
            return avaliacao;
        }

        public override void ExibirPainel(NexusFitSystem sistema)
        {
            while (true)
            {
                Console.WriteLine("\n==========================================");
                Console.WriteLine($"PAINEL DO INSTRUTOR — {Nome}");
                Console.WriteLine("==========================================");
                Console.WriteLine("1. Cadastrar Novo Membro");
                Console.WriteLine("2. Consultar Dados Cadastrais dos Membros");
                Console.WriteLine("3. Consultar Pendências (Ativo / Inativo)");
                Console.WriteLine("4. Efetuar Cobrança de Membro");
                Console.WriteLine("5. Cancelar Matrícula de Membro");
                Console.WriteLine("6. Montar Treino para Membro");
                Console.WriteLine("7. Registrar Avaliação Física");
                Console.WriteLine("8. Consultar Evolução de um Membro");
                Console.WriteLine("0. Sair / Logout");
                Console.Write("Escolha: ");

                string? opc = Console.ReadLine()?.Trim();
                if (opc == "0") break;

                try
                {
                    switch (opc)
                    {
                        case "1": sistema.CadastrarMembroViaConsole(); break;
                        case "2": sistema.ListarDadosMembros(); break;
                        case "3": ConsultarPendencias(sistema.ObterMembros()); break;
                        case "4":
                            var mCob = sistema.BuscarMembroPorMatriculaViaConsole();
                            EfetuarCobranca(mCob);
                            Console.WriteLine("Cobrança efetuada!");
                            break;
                        case "5":
                            var mCanc = sistema.BuscarMembroPorMatriculaViaConsole();
                            CancelarMatricula(mCanc);
                            Console.WriteLine("Matrícula cancelada!");
                            break;
                        case "6": sistema.MontarTreinoViaConsole(this); break;
                        case "7": sistema.RegistrarAvaliacaoViaConsole(this); break;
                        case "8":
                            var mEvo = sistema.BuscarMembroPorMatriculaViaConsole();
                            mEvo.ConsultarEvolucao();
                            break;
                        default: Console.WriteLine("Opção inválida."); break;
                    }
                }
                catch (Exception ex) when (ex is RegraNegocioException or FormatException or ArgumentOutOfRangeException)
                {
                    Console.WriteLine($"[ERRO] {ex.Message}");
                }
            }
        }
    }
}
