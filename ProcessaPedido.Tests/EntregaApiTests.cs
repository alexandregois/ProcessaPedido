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
        private const string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsInJvbGUiOiJBZG1pbiIsImV4cCI6MTcxNzQzMTYwMH0.2Qn6QwQw6QwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQw";

        public EntregaApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");
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
            var postResult = await postResponse.Content.ReadFromJsonAsync<TempIdResult>();
            Assert.NotNull(postResult);
            Assert.NotEqual(System.Guid.Empty, postResult.Id);

            // GET
            var getResponse = await _client.GetAsync($"/entregas/{postResult.Id}");
            Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.StatusCode);
            var entrega = await getResponse.Content.ReadFromJsonAsync<EntregaResult>();
            Assert.NotNull(entrega);
            Assert.Equal(payload.PedidoId, entrega.PedidoId);
            Assert.Equal(payload.Destinatario.Nome, entrega.Destinatario.Nome);
            Assert.Equal(payload.Itens.Count, entrega.Itens.Count);

            Assert.Equal(StatusEntrega.Pendente, (StatusEntrega)entrega.Status);

            // Aguarda processamento assíncrono
            Thread.Sleep(4000);
            var getResponse2 = await _client.GetAsync($"/entregas/{postResult.Id}");
            var entrega2 = await getResponse2.Content.ReadFromJsonAsync<EntregaResult>();
            Console.WriteLine($"Status final recebido: {entrega2.Status}");
            Assert.True(
                entrega2.Status == (int)StatusEntrega.SaiuParaEntrega ||
                entrega2.Status == (int)StatusEntrega.Entregue ||
                entrega2.Status == (int)StatusEntrega.FalhaNaEntrega
            );
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
            var postResult = await postResponse.Content.ReadFromJsonAsync<TempIdResult>();
            Assert.NotNull(postResult);
            Assert.NotEqual(System.Guid.Empty, postResult.Id);

            // GET (status inicial)
            var getResponse = await _client.GetAsync($"/entregas/{postResult.Id}");
            Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.StatusCode);
            var entrega = await getResponse.Content.ReadFromJsonAsync<EntregaResult>();
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
            Thread.Sleep(4000);
            var getResponse2 = await _client.GetAsync($"/entregas/{postResult.Id}");
            var entrega2 = await getResponse2.Content.ReadFromJsonAsync<EntregaResult>();
            Console.WriteLine($"Status final recebido: {entrega2.Status}");
            Assert.True(
                entrega2.Status == (int)StatusEntrega.SaiuParaEntrega ||
                entrega2.Status == (int)StatusEntrega.Entregue ||
                entrega2.Status == (int)StatusEntrega.FalhaNaEntrega
            );
        }

        private class TempIdResult { public System.Guid Id { get; set; } }
        private class EntregaResult
        {
            public string PedidoId { get; set; }
            public DestinatarioDto Destinatario { get; set; }
            public System.Collections.Generic.List<ItemEntregaDto> Itens { get; set; }
            public int Status { get; set; }
        }
    } 
}