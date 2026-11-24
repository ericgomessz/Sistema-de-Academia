# Sobre 
## Sistema-de-Academia / Descrição 

Neste projeto iremos desenvolver um sistemas para usuários de uma academia, controle de matricula/login , pagamento, fidelidade, pendencias, planos e demais funcionalidades.

## Integrantes

* Eduardo Antunes
* Eric Gomes
* Gustavo de Aguilar
* Jefferson Marlon

## Professor
     
* Jardell Fillipe da Silva           

## Documentação

<ol>
<li><a href="Documentação.md"> Documentação de contexto</a></li>
<li><a href="Modelagem.md"> Modelagem dos processos de negócios</a></li>
<li><a href="Prototipo.md"> Projeto de interface</a></li>
<li><a href="Comportamento.md"> Comportamento</a></li>
<li><a href="Program.cs">Código</a></li>

## Instruções de utilização.

## 🚀 Como Executar o Projeto

### Pré-requisitos
* [.NET SDK](https://dotnet.microsoft.com/download) instalado (versão 8.0 ou superior).
* Terminal (Bash, PowerShell, CMD ou terminal integrado do VS Code).

### Passo a passo no terminal

1. **Clone o repositório:**

   git clone [https://github.com/ogomesz/Sistema-de-Academia.git](https://github.com/ogomesz/Sistema-de-Academia.git)
   cd Sistema-de-Academia

   dotnet run

 ## 🔑 Contas de Teste (Dados Iniciais)

O sistema inicia com três perfis pré-cadastrados para facilitar os testes e a apresentação:

| Perfil | E-mail | Senha | Permissões no Painel |
| :--- | :--- | :--- | :--- |
| **Administrador (Funcionário)** | `admin@nexusfit.com` | `admin123` | Cadastrar membros, consultar dados e pendências (`ATIVO`/`INATIVO`), efetuar cobranças e cancelar matrículas. |
| **Instrutor** | `instrutor@nexusfit.com` | `treino123` | Todas as funções de funcionário + montar treinos e registrar avaliações físicas. |
| **Membro (Aluno)** | `lucas@email.com` | `aluno123` | Contratar/renovar/alterar plano, solicitar trancamento, consultar pagamentos, ver treinos e acompanhar evolução física. |
