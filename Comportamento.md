# Pilares de POO no NexusFit

Como os quatro pilares trabalham no sistema, alinhados ao diagrama de classes (`diagrama-classes.md`).

---

## 1. Abstração

### Classes abstratas

- **`Usuario`:** não existem "usuários genéricos". Todo indivíduo cadastrado é obrigatoriamente um `Membro`, um `Funcionario` ou um `Instrutor`.
- **`Plano`:** serve como modelo base. Na prática, o contrato sempre será um `PlanoMensal`, `PlanoTrimestral` ou `PlanoAnual`, combinado a uma `Categoria` (Prata, Ouro ou Diamante).
- **`Exercicio`:** modelo base dos exercícios. Na prática, todo exercício de um treino é `Aerobico` ou `Musculacao`.

**Por que abstrair?** Isola as regras essenciais (autenticação, cobrança de planos e descrição de exercícios) e padroniza os comportamentos comuns, deixando para cada subclasse apenas o que é específico dela.

**O que foi ignorado (fora do escopo):** integração física com hardware de catracas, exames médicos detalhados e transações bancárias em tempo real.

---

## 2. Encapsulamento

### Atributos privados/protegidos

| Atributo | Classe | Acesso |
|---|---|---|
| `SenhaHash` | `Usuario` | `private`, nunca exibido |
| `Cpf` | `Usuario` | `private`, exposto por propriedade somente leitura |
| `Status` (ativo/inativo) | `Membro` | `private set` |
| `AulasLivresRestantesMes` | `Membro` | `private`, alterado só por método |
| `ConvitesAmigosRestantesMes` | `Membro` | `private`, alterado só por método |
| `Salario` | `Funcionario` | `private`, acessado só por quem tem permissão |

### Controle de acesso

Feito por **propriedades (get/set)** e por **métodos específicos** da classe. Nenhum atributo sensível é alterado diretamente de fora.

### Validações aplicadas

- **Senha:** impede strings vazias ou com menos de 6 caracteres. Depois de validada, é armazenada como **hash**, e nem o funcionário consegue visualizá-la.
- **CPF:** validado no momento do cadastro (formato e dígitos verificadores).
- **Status (ativo/inativo):** só pode ser alterado por métodos do `Funcionario` ou mediante baixa de pagamento.
- **Convites de amigos:** `UsarConvite()` decrementa o saldo do mês e **impede valores negativos**, lançando uma exceção tratada quando o limite acaba.
- **Aula de livre escolha:** `UsarAulaLivre()` segue a mesma lógica, respeitando o limite mensal da categoria.

---

## 3. Herança

### Hierarquia 1: atores do sistema

- **Classe-mãe: `Usuario`** reúne `Nome`, `Email`, `Cpf`, `Senha` e `Autenticar()`.
- **Filhas:**
  - **`Membro`:** adiciona `Matricula`, `Status`, `DataAdesao` e operações como `ContratarPlano()`, `RenovarPlano()`, `AlterarPlano()`, `SolicitarTrancamento()`, `ConsultarPagamentos()` e `ConsultarEvolucao()`.
  - **`Funcionario`:** adiciona `Cargo`, `Salario`, `ConsultarPendencias()`, `EfetuarCobranca()` e `CancelarMatricula()`.
  - **`Instrutor`** (herda de `Funcionario`): adiciona `MontarTreino()` e `RegistrarAvaliacao()`.

### Hierarquia 2: modalidades de contrato

- **Classe-mãe: `Plano`** define a `Categoria` e os métodos abstratos `CalcularMensalidade()` e `PermiteTrancamento()`.
- **Filhas:**
  - **`PlanoMensal`:** sem desconto de duração, não permite trancamento.
  - **`PlanoTrimestral`:** desconto de duração, permite trancamento.
  - **`PlanoAnual`:** maior desconto de duração, permite trancamento.

### Hierarquia 3: exercícios

- **Classe-mãe: `Exercicio`** define `Nome` e o método abstrato `Descrever()`.
- **Filhas:**
  - **`Aerobico`:** acrescenta `DuracaoMinutos` e `Intensidade`.
  - **`Musculacao`:** acrescenta `Series`, `Repeticoes` e `Carga`.

### Categorias de benefício (`Categoria`)

Em vez de subclasses, os benefícios de acesso ficam em um enum aplicado a qualquer plano:

| Categoria | Benefícios |
|---|---|
| **Diamante** | 7 dias/semana, aulas de Bike e Box, +3 convites de amigos por mês |
| **Ouro** | 7 dias/semana, aulas de Bike e Box |
| **Prata** | 5 dias/semana e 1 aula avulsa por mês |

---

## 4. Polimorfismo

### Método base 1: `ExibirPainel()` em `Usuario`

- **No `Membro`:** renderiza a tela do cliente (status da matrícula, pagamentos, plano, treinos e evolução física).
- **No `Funcionario`:** renderiza o painel administrativo (consulta de dados, pendências, cobrança e cancelamento de matrícula).
- **No `Instrutor`:** estende o painel do funcionário com a montagem de treinos e o registro de avaliações físicas.

### Método base 2: `CalcularMensalidade()` e `PermiteTrancamento()` em `Plano`

| Plano | `CalcularMensalidade(mesesFidelidade)` | `PermiteTrancamento()` |
|---|---|---|
| `PlanoMensal` | Valor base da categoria, sem desconto de duração | `false` |
| `PlanoTrimestral` | Valor base com desconto trimestral e desconto de fidelidade | `true` |
| `PlanoAnual` | Valor base com desconto anual e desconto de fidelidade | `true` |

Uma lista de `Plano` pode ser percorrida sem saber o tipo concreto de cada item: cada objeto responde do seu jeito.

### Método base 3: `Descrever()` em `Exercicio`

- **`Aerobico`:** descreve duração e intensidade (ex.: "Esteira, 30 min, intensidade moderada").
- **`Musculacao`:** descreve séries, repetições e carga (ex.: "Supino, 4x10, 40 kg").

O `Treino` guarda uma lista de `Exercicio` e chama `Descrever()` em cada um para montar a ficha do membro.

### Validação de acesso por categoria

O acesso a aulas e convites é validado a partir da `Categoria` do plano ativo:

- **Diamante:** libera 7 dias/semana, Bike, Box e entrada de convidados (até 3 por mês).
- **Ouro:** libera 7 dias/semana, Bike e Box.
- **Prata:** restringe a 5 dias/semana e valida o limite de 1 aula avulsa por mês.
