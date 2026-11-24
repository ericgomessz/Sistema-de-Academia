using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NexusFit.Models
{
    public class NexusFitSystem
    {
        private readonly List<Usuario> _usuarios = new();

        public NexusFitSystem()
        {
            SeedDadosIniciais();
        }

        public IEnumerable<Membro> ObterMembros() => _usuarios.OfType<Membro>();

        public Usuario AutenticarUsuario(string identificador, string senha)
        {
            if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrEmpty(senha))
                throw new AutenticacaoException("E-mail/CPF e senha são obrigatórios.");

            string idLimpo = identificador.Trim().ToLowerInvariant();
            string cpfLimpo = new string(identificador.Where(char.IsDigit).ToArray());

            var usuario = _usuarios.FirstOrDefault(u =>
                u.Email.Equals(idLimpo, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(cpfLimpo) && new string(u.Cpf.Where(char.IsDigit).ToArray()) == cpfLimpo));

            if (usuario == null || !usuario.Autenticar(senha))
                throw new AutenticacaoException("Credenciais inválidas.");

            return usuario;
        }

        public Plano LerEscolhaPlano()
        {
            Console.WriteLine("\n--- Escolha a Categoria ---");
            Console.WriteLine($"1. Prata    (Base R$ {RegrasNegocioConfig.ObterValorBaseCategoria(CategoriaPlano.Prata):F2}) - {RegrasNegocioConfig.DescreverBeneficios(CategoriaPlano.Prata)}");
            Console.WriteLine($"2. Ouro     (Base R$ {RegrasNegocioConfig.ObterValorBaseCategoria(CategoriaPlano.Ouro):F2}) - {RegrasNegocioConfig.DescreverBeneficios(CategoriaPlano.Ouro)}");
            Console.WriteLine($"3. Diamante (Base R$ {RegrasNegocioConfig.ObterValorBaseCategoria(CategoriaPlano.Diamante):F2}) - {RegrasNegocioConfig.DescreverBeneficios(CategoriaPlano.Diamante)}");
            Console.Write("Categoria (1-3): ");
            int opcCat = LerInteiro();

            CategoriaPlano categoria = opcCat switch
            {
                1 => CategoriaPlano.Prata,
                2 => CategoriaPlano.Ouro,
                3 => CategoriaPlano.Diamante,
                _ => throw new RegraNegocioException("Categoria inválida.")
            };

            Console.WriteLine("\n--- Escolha a Duração ---");
            Console.WriteLine("1. Mensal (sem desconto, sem trancamento)");
            Console.WriteLine($"2. Trimestral ({RegrasNegocioConfig.DescontoTrimestral * 100:0}% desc., permite trancamento)");
            Console.WriteLine($"3. Anual ({RegrasNegocioConfig.DescontoAnual * 100:0}% desc., permite trancamento)");
            Console.Write("Duração (1-3): ");
            int opcDur = LerInteiro();

            return opcDur switch
            {
                1 => new PlanoMensal(categoria),
                2 => new PlanoTrimestral(categoria),
                3 => new PlanoAnual(categoria),
                _ => throw new RegraNegocioException("Duração inválida.")
            };
        }

        public void CadastrarMembroViaConsole()
        {
            Console.Write("Nome completo: ");
            string nome = Console.ReadLine() ?? string.Empty;

            Console.Write("E-mail: ");
            string email = Console.ReadLine() ?? string.Empty;

            Console.Write("CPF: ");
            string cpf = Console.ReadLine() ?? string.Empty;

            Console.Write("Senha (mín. 6 caracteres): ");
            string senha = Console.ReadLine() ?? string.Empty;

            Console.Write("Matrícula: ");
            string matricula = Console.ReadLine() ?? string.Empty;

            var novoMembro = new Membro(nome, email, cpf, senha, matricula);
            _usuarios.Add(novoMembro);
            Console.WriteLine($"Membro '{novoMembro.Nome}' cadastrado com matrícula {novoMembro.Matricula}!");
        }

        public void ListarDadosMembros()
        {
            Console.WriteLine("\n--- Dados Cadastrais dos Membros ---");
            var membros = ObterMembros().ToList();
            if (membros.Count == 0)
            {
                Console.WriteLine("Nenhum membro cadastrado.");
                return;
            }

            foreach (var m in membros)
            {
                string planoInfo = m.PlanoAtivo != null ? $"{m.PlanoAtivo.DescricaoDuracao} ({m.PlanoAtivo.Categoria})" : "Sem plano";
                Console.WriteLine($"Matrícula: {m.Matricula} | Nome: {m.Nome} | E-mail: {m.Email} | CPF: {m.Cpf} | Status: {m.Status} | Plano: {planoInfo}");
            }
        }

        public Membro BuscarMembroPorMatriculaViaConsole()
        {
            Console.Write("Digite a matrícula do membro: ");
            string mat = (Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();

            var membro = ObterMembros().FirstOrDefault(m => m.Matricula == mat);
            if (membro == null)
                throw new RegraNegocioException($"Membro com matrícula '{mat}' não encontrado.");

            return membro;
        }

        public void MontarTreinoViaConsole(Instrutor instrutor)
        {
            var membro = BuscarMembroPorMatriculaViaConsole();
            Console.Write("Nome do treino (ex: Treino A - Peito/Tríceps): ");
            string nomeTreino = Console.ReadLine() ?? string.Empty;

            var exercicios = new List<Exercicio>();
            Console.Write("Quantos exercícios deseja adicionar? ");
            int qtd = LerInteiro();
            if (qtd <= 0)
                throw new RegraNegocioException("Informe ao menos 1 exercício.");

            for (int i = 0; i < qtd; i++)
            {
                Console.WriteLine($"\nExercício {i + 1}: 1 - Musculação | 2 - Aeróbico");
                int tipo = LerInteiro();
                Console.Write("Nome do exercício: ");
                string nomeEx = Console.ReadLine() ?? string.Empty;

                if (tipo == 1)
                {
                    Console.Write("Séries: ");
                    int series = LerInteiro();
                    Console.Write("Repetições: ");
                    int reps = LerInteiro();
                    Console.Write("Carga (kg): ");
                    decimal carga = LerDecimal();
                    exercicios.Add(new Musculacao(nomeEx, series, reps, carga));
                }
                else if (tipo == 2)
                {
                    Console.Write("Duração (minutos): ");
                    int duracao = LerInteiro();
                    Console.Write("Intensidade (ex: Leve, Moderada, Alta): ");
                    string intens = Console.ReadLine() ?? string.Empty;
                    exercicios.Add(new Aerobico(nomeEx, duracao, intens));
                }
                else
                {
                    throw new RegraNegocioException("Tipo de exercício inválido.");
                }
            }

            instrutor.MontarTreino(membro, nomeTreino, exercicios);
            Console.WriteLine("Treino montado e associado ao membro com sucesso!");
        }

        public void RegistrarAvaliacaoViaConsole(Instrutor instrutor)
        {
            var membro = BuscarMembroPorMatriculaViaConsole();
            Console.Write("Peso (kg): ");
            decimal peso = LerDecimal();
            Console.Write("Cintura (cm): ");
            decimal cintura = LerDecimal();
            Console.Write("Quadril (cm): ");
            decimal quadril = LerDecimal();
            Console.Write("Tórax (cm): ");
            decimal torax = LerDecimal();
            Console.Write("Braço (cm): ");
            decimal braco = LerDecimal();

            var medidas = new MedidasCorporais(cintura, quadril, torax, braco);
            instrutor.RegistrarAvaliacao(membro, DateTime.Today, peso, medidas);
            Console.WriteLine("Avaliação física registrada com sucesso!");
        }

        public static int LerInteiro()
        {
            string entrada = (Console.ReadLine() ?? string.Empty).Trim();
            if (!int.TryParse(entrada, out int valor))
                throw new FormatException("Digite um número inteiro válido.");
            return valor;
        }

        public static decimal LerDecimal()
        {
            string entrada = (Console.ReadLine() ?? string.Empty).Trim().Replace(',', '.');
            if (!decimal.TryParse(entrada, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
                throw new FormatException("Digite um número válido.");
            return valor;
        }

        private void SeedDadosIniciais()
        {
            var admin = new Funcionario("Carlos Mendes", "admin@nexusfit.com", "52998224725", "admin123", "Coordenador", 4200m);
            var instrutor = new Instrutor("Ana Paula", "instrutor@nexusfit.com", "11144477735", "treino123", 3200m);
            var membro = new Membro("Lucas Silva", "lucas@email.com", "04628094083", "aluno123", "NX001");
            membro.ContratarPlano(new PlanoTrimestral(CategoriaPlano.Ouro));

            _usuarios.Add(admin);
            _usuarios.Add(instrutor);
            _usuarios.Add(membro);
        }
    }
}
