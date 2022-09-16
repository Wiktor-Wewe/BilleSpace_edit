using BilleSpace.Domain.CQRS;
using BilleSpace.Domain.Results;
using BilleSpace.Infrastructure.Entities;
using BilleSpace.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace BilleSpace.IntegrationTests.E2E_Office_Tests
{
    [Category("E2E_Office_Test")]
    public class E2E_Office_Test_With_Correct_Data : Setup
    {
        [Test]
        public async Task E2E_Office_With_Correct_Data_Test()
        {
            // Arrange
            AuthenticateAsync();

            City city = new City()
            {
                Name = "Olsztyn",
                Country = new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Poland",
                    Symbol = "PL"
                }
            };

            var modelOffice = new Office()
            {
                Id = Guid.NewGuid(),
                City = city,
                Address = "Olsztyn",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZone> { new OfficeZone
                {
                    Name = "OfficeZone",
                    Desks = 5,
                } },
                ParkingZones = new List<ParkingZone>(),
                OfficeMapUrl = null,
                AuthorEmail = "Author@wp.pl"
            };

            var addModel = new ManageOfficeCommand
            {
                Id = modelOffice.Id,
                City = city.Name,
                Address = "Warszawa",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZoneModel> {
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone2",
                        Desks = 5,
                    },
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone3",
                        Desks = 15,
                    }
                },
                ParkingZones = new List<ParkingZoneModel>(),
                OfficeMap = null,
                AuthorEmail = "Zdzisiek@wp.pl"
            };


            var updateModel = new ManageOfficeCommand
            {
                Id = modelOffice.Id,
                City = city.Name,
                Address = "Olsztyn",
                PostCode = "Olsztyn",
                OfficeZones = new List<OfficeZoneModel> {
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone2",
                        Desks = 5,
                    },
                    new OfficeZoneModel
                    {
                        Name = "OfficeZone3",
                        Desks = 15,
                    }
                },
                ParkingZones = new List<ParkingZoneModel>(),
                OfficeMap = null,
                AuthorEmail = "Zdzisiek@wp.pl"
            };

            //act
            //add
            var response = await _httpClient.PostAsync("api/Offices", new StringContent(JsonConvert.SerializeObject(addModel), Encoding.UTF8, "application/json"));
            var addResponse = await response.Content.ReadAsAsync<Result<OfficeModel>>();

            //getAllOffices
            var getOffices = await _httpClient.GetAsync($"api/Offices/");
            var getOfficesResponse = await getOffices.Content.ReadAsAsync<Result<List<OfficeModel>>>();

            //getOfficeById
            var getOfficebyId = await _httpClient.GetAsync($"api/Offices/{addResponse.Data.Id}");
            var getOfficebyIdResponse = await getOfficebyId.Content.ReadAsAsync<Result<OfficeModel>>();

            //update
            var upresponse = await _httpClient.PutAsync($"api/Offices/{addResponse.Data.Id}", new StringContent(JsonConvert.SerializeObject(updateModel), Encoding.UTF8, "application/json"));
            var updateResponse = await upresponse.Content.ReadAsAsync<Result<OfficeModel>>();

            //getOfficeByIdAfterUpdate
            var getOfficebyIdAfterUpdate = await _httpClient.GetAsync($"api/Offices/{addResponse.Data.Id}");
            var getOfficebyIdResponseAfterUpdate = await getOfficebyIdAfterUpdate.Content.ReadAsAsync<Result<OfficeModel>>();

            //del 
            var delresult = await _httpClient.DeleteAsync($"api/Offices/{updateResponse.Data.Id}");


            //assert

            //add
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(addResponse.Code, Is.EqualTo(200));
            Assert.That(addResponse.Data.Address, Is.EqualTo("Warszawa"));
            Assert.That(addResponse.Errors, Is.Null);

            //getAllOffices
            Assert.That(getOffices.IsSuccessStatusCode, Is.True);
            Assert.That(getOfficesResponse.Code, Is.EqualTo(200));
            Assert.That(getOfficesResponse.Errors, Is.Null);

            //getOfficeById
            Assert.That(getOfficebyId.IsSuccessStatusCode, Is.True);
            Assert.That(getOfficebyIdResponse.Code, Is.EqualTo(200));
            Assert.That(getOfficebyIdResponse.Data.Address, Is.EqualTo("Warszawa"));
            Assert.That(getOfficebyIdResponse.Errors, Is.Null);

            //update
            Assert.That(upresponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updateResponse.Code, Is.EqualTo(200));
            Assert.That(updateResponse.Data.Address, Is.EqualTo("Olsztyn"));
            Assert.That(updateResponse.Errors, Is.Null);

            //getOfficeByIdgetOfficebyIdAfterUpdate
            Assert.That(getOfficebyIdAfterUpdate.IsSuccessStatusCode, Is.True);
            Assert.That(getOfficebyIdResponseAfterUpdate.Code, Is.EqualTo(200));
            Assert.That(getOfficebyIdResponseAfterUpdate.Data.Address, Is.EqualTo("Olsztyn"));
            Assert.That(getOfficebyIdResponseAfterUpdate.Errors, Is.Null);

            //del
            Assert.That(delresult.IsSuccessStatusCode, Is.True);
        }
    }
}
