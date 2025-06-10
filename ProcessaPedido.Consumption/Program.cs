using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProcessaPedido.Consumption
{
    public class EntregaCreateDto
    {
        public string PedidoId { get; set; }
        public DestinatarioDto Destinatario { get; set; }
        public List<ItemEntregaDto> Itens { get; set; }
    }
    public class DestinatarioDto
    {
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string Cep { get; set; }
    }
    public class ItemEntregaDto
    {
        public string Descricao { get; set; }
        public int Quantidade { get; set; }
    }

    class Program
    {
        private static readonly string apiBaseUrl = "http://localhost:5272"; // Ajuste se necessário
        private static readonly HttpClient client = new HttpClient();
        private static string _token;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Consumidor iniciado. Pressione Ctrl+C para sair.");
            await AuthenticateAsync();
            while (true)
            {
                await PostEntregaAsync();
                await GetAllEntregasAsync();
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        static async Task AuthenticateAsync()
        {
            var login = new { username = "admin", password = "admin" };
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/auth/login", login);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                _token = doc.RootElement.GetProperty("token").GetString();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                Console.WriteLine($"[AUTH] Token obtido com sucesso.");
            }
            else
            {
                Console.WriteLine($"[AUTH] Falha ao autenticar: {response.StatusCode}");
            }
        }

        static async Task PostEntregaAsync()
        {
            var entrega = new EntregaCreateDto
            {
                PedidoId = Guid.NewGuid().ToString().Substring(0, 8),
                Destinatario = new DestinatarioDto
                {
                    Nome = "Cliente Teste",
                    Endereco = "Rua Maria Lisboa, 110 - São Gonçalo - RJ",
                    Cep = "24420-000"
                },
                Itens = new List<ItemEntregaDto>
                {
                    new ItemEntregaDto { Descricao = "Tinta Suvinil - Galão 18l", Quantidade = 1 },
                    new ItemEntregaDto { Descricao = "Rolo p/ Textura", Quantidade = 2 }
                }
            };
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/entregas", entrega);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[POST] Entrega criada: {result}");
            }
            else
            {
                Console.WriteLine($"[POST] Falha ao criar entrega: {response.StatusCode}");
            }
        }

        static async Task GetEntregaAsync(Guid? id = null, string pedidoId = null)
        {
            var url = $"{apiBaseUrl}/entregas?";
            if (id != null) url += $"id={id}&";
            if (!string.IsNullOrEmpty(pedidoId)) url += $"pedidoId={pedidoId}&";
            var response = await client.GetAsync(url.TrimEnd('&'));
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GET] Entrega: {result}");
            }
            else
            {
                Console.WriteLine($"[GET] Falha ao buscar entrega: {response.StatusCode}");
            }
        }

        static async Task GetAllEntregasAsync()
        {
            var response = await client.GetAsync($"{apiBaseUrl}/entregas/all");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GET ALL] Entregas: {result}");
            }
            else
            {
                Console.WriteLine($"[GET ALL] Falha ao buscar entregas: {response.StatusCode}");
            }
        }
    }
}
