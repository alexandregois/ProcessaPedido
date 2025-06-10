using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Linq;
using System.Threading;
using ProcessaPedido.Domain.Entities;
using ProcessaPedido.Domain.Enum;
using ProcessaPedido.Api.Models;

namespace ProcessaPedido.Tests
{
    public class EntregaApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public EntregaApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            AuthenticateAsync().GetAwaiter().GetResult();
        }

        private async Task AuthenticateAsync()
        {
            var login = new { username = "admin", password = "admin" };
            var response = await _client.PostAsJsonAsync("/auth/login", login);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task PostAndGetEntrega_Success()
        {
            var payload = new EntregaCreateDto
            {
                PedidoId = "ABC123",
                Destinatario = new DestinatarioDto
                {
                    Nome = "João da Silva",
                    Endereco = "Rua das Flores, 1000",
                    Cep = "01010-000"
                },
                Itens = new System.Collections.Generic.List<ItemEntregaDto>
               {
                   new ItemEntregaDto { Descricao = "Geladeira", Quantidade = 1 },
                   new ItemEntregaDto { Descricao = "Fogão", Quantidade = 1 }
               }
            };

            // POST
            var postResponse = await _client.PostAsJsonAsync("/entregas", payload);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, postResponse.StatusCode);
            var postResult = await postResponse.Content.ReadFromJsonAsync<TempIdResult>() ?? throw new InvalidOperationException("Falha ao deserializar resposta");
            Assert.NotNull(postResult);
            Assert.NotEqual(System.Guid.Empty, postResult.Id);

            // GET
            var getResponse = await _client.GetAsync($"/entregas/{postResult.Id}");
            getResponse.EnsureSuccessStatusCode();
            var entrega = await getResponse.Content.ReadFromJsonAsync<EntregaResult>() ?? throw new InvalidOperationException("Falha ao deserializar resposta");
            Assert.NotNull(entrega);
            Assert.Equal(payload.PedidoId, entrega.PedidoId);
            Assert.Equal(payload.Destinatario.Nome, entrega.Destinatario.Nome);
            Assert.Equal(payload.Itens.Count, entrega.Itens.Count);

            // Aguarda processamento assíncrono
            Thread.Sleep(2000); // Aguarda 2 segundos para o RabbitMQ processar

            // Verifica status após processamento
            var getResponse2 = await _client.GetAsync($"/entregas/{postResult.Id}");
            getResponse2.EnsureSuccessStatusCode();
            var entrega2 = await getResponse2.Content.ReadFromJsonAsync<EntregaResult>() ?? throw new InvalidOperationException("Falha ao deserializar resposta");
            Assert.Equal(2, entrega2.Status); // SaiuParaEntrega
        }

        [Fact]
        public async Task PostAndGetEntrega_SecondOrder_Success()
        {
            var payload = new EntregaCreateDto
            {
                PedidoId = "XYZ789",
                Destinatario = new DestinatarioDto
                {
                    Nome = "Maria Oliveira",
                    Endereco = "Avenida Central, 200",
                    Cep = "02020-000"
                },
                Itens = new System.Collections.Generic.List<ItemEntregaDto>
                {
                    new ItemEntregaDto { Descricao = "TV", Quantidade = 2 },
                    new ItemEntregaDto { Descricao = "Micro-ondas", Quantidade = 1 }
                }
            };

            // POST
            var postResponse = await _client.PostAsJsonAsync("/entregas", payload);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, postResponse.StatusCode);
            var postResult = await postResponse.Content.ReadFromJsonAsync<TempIdResult>() ?? throw new InvalidOperationException("Falha ao deserializar resposta");
            Assert.NotNull(postResult);
            Assert.NotEqual(System.Guid.Empty, postResult.Id);

            // GET (status inicial)
            var getResponse = await _client.GetAsync($"/entregas/{postResult.Id}");
            getResponse.EnsureSuccessStatusCode();
            var entrega = await getResponse.Content.ReadFromJsonAsync<EntregaResult>() ?? throw new InvalidOperationException("Falha ao deserializar resposta");
            Assert.NotNull(entrega);
            Assert.Equal(payload.PedidoId, entrega.PedidoId);
            Assert.Equal(payload.Destinatario.Nome, entrega.Destinatario.Nome);
            Assert.Equal(payload.Itens.Count, entrega.Itens.Count);
            // Aceita Pendente ou SaiuParaEntrega devido ao processamento assíncrono
            Assert.True(
                entrega.Status == (int)StatusEntrega.Pendente ||
                entrega.Status == (int)StatusEntrega.SaiuParaEntrega
            );

            // Aguarda processamento assíncrono
            Thread.Sleep(2000); // Aguarda 2 segundos para o RabbitMQ processar

            // Verifica status após processamento
            var getResponse2 = await _client.GetAsync($"/entregas/{postResult.Id}");
            getResponse2.EnsureSuccessStatusCode();
            var entrega2 = await getResponse2.Content.ReadFromJsonAsync<EntregaResult>() ?? throw new InvalidOperationException("Falha ao deserializar resposta");
            Assert.Equal(2, entrega2.Status); // SaiuParaEntrega
        }

        private class TempIdResult { public System.Guid Id { get; set; } }

        private class EntregaResult
        {
            public string PedidoId { get; set; }
            public DestinatarioDto Destinatario { get; set; }
            public System.Collections.Generic.List<ItemEntregaDto> Itens { get; set; }
            public int Status { get; set; }
        }

        public class EntregaCreateDto
        {
            public required string PedidoId { get; set; }
            public required DestinatarioDto Destinatario { get; set; }
            public required List<ItemEntregaDto> Itens { get; set; }
        }

        public class DestinatarioDto
        {
            public required string Nome { get; set; }
            public required string Endereco { get; set; }
            public required string Cep { get; set; }
        }

        public class ItemEntregaDto
        {
            public required string Descricao { get; set; }
            public int Quantidade { get; set; }
        }
    } 
}