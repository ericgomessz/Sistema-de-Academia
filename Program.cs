using System;
using System.Text;
using NexusFit.Models;

namespace NexusFit
{
    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var sistema = new NexusFitSystem();

            while (true)
            {
                Console.WriteLine("\n==========================================");
                Console.WriteLine("        NEXUSFIT — ACESSO AO SISTEMA");
                Console.WriteLine("==========================================");
                Console.WriteLine("Contas de teste: lucas@email.com / aluno123 | admin@nexusfit.com / admin123 | instrutor@nexusfit.com / treino123");
                Console.WriteLine("1. Fazer Login");
                Console.WriteLine("0. Encerrar");
                Console.Write("Escolha: ");

                string? opc = Console.ReadLine()?.Trim();
                if (opc == "0") break;

                if (opc != "1")
                {
                    Console.WriteLine("Opção inválida.");
                    continue;
                }

                try
                {
                    Console.Write("E-mail ou CPF: ");
                    string login = Console.ReadLine() ?? string.Empty;

                    Console.Write("Senha: ");
                    string senha = Console.ReadLine() ?? string.Empty;

                    Usuario usuarioAutenticado = sistema.AutenticarUsuario(login, senha);
                    Console.WriteLine($"\nBem-vindo(a), {usuarioAutenticado.Nome}!");

                    usuarioAutenticado.ExibirPainel(sistema);
                }
                catch (AutenticacaoException ex)
                {
                    Console.WriteLine($"[FALHA DE LOGIN] {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO INESPERADO] {ex.Message}");
                }
            }
        }
    }
}
