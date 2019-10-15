using Altinn.Platform.Storage.Configuration;
using Altinn.Platform.Storage.Controllers;
using Altinn.Platform.Storage.Models;
using Altinn.Platform.Storage.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Storage.Interface.Clients;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Altinn.Platform.Storage.UnitTest
{
    public class InstanceOwnerLookupTest
    {
        InstancesController instanceController;

        public InstanceOwnerLookupTest()
        {            
        }

        public async void InstanceLookupPersonNumberTest()
        {
            Instance instanceToCreate = new Instance()
            {
                Id = "1001/5650b227-5f79-41d8-a901-abed492c6fd4",
                AppId = "test/lookup",
                Org = "test"
            };

            Instance instanceTemplate = new Instance()
            {
                InstanceOwnerLookup = new InstanceOwnerLookup()
                {
                    PersonNumber = "1",
                }
            };

            PrepareLookupMock(HttpStatusCode.OK, "1001", instanceToCreate, instanceTemplate);

            ActionResult result = await instanceController.Post("test/appid", null);

            OkObjectResult okresult = result as OkObjectResult;

            Instance resultInstance = (Instance)okresult.Value;

            Assert.NotNull(resultInstance);
            Assert.Equal(instanceToCreate.Id, resultInstance.Id);
            Assert.Null(resultInstance.InstanceOwnerLookup);
        }

        [Fact]
        public async void InstanceLookupPersonNumberFails()
        {
            Instance instanceTemplate = new Instance()
            {
                InstanceOwnerLookup = new InstanceOwnerLookup()
                {
                    PersonNumber = "unknown"
                }
            };

            PrepareLookupMock(HttpStatusCode.BadRequest, "fails", null, instanceTemplate);

            ActionResult result = await instanceController.Post("test/appid", null);

            BadRequestObjectResult badResult = result as BadRequestObjectResult;

            Assert.NotNull(badResult);
        }

        public async void InstanceOrganisationNumberTest()
        {
            Instance instanceToCreate = new Instance()
            {
                Id = "500004690/5650b227-5f79-41d8-a901-abed492c6fd4",
                AppId = "test/lookup",
                Org = "test"
            };

            Instance instanceTemplate = new Instance()
            {
                InstanceOwnerLookup = new InstanceOwnerLookup()
                {
                    OrganisationNumber = "910434462",
                }
            };

            PrepareLookupMock(HttpStatusCode.OK, "50004690", instanceToCreate, instanceTemplate);

            ActionResult result = await instanceController.Post("test/appid", null);

            OkObjectResult okresult = result as OkObjectResult;

            Instance resultInstance = (Instance)okresult.Value;

            Assert.NotNull(resultInstance);
            Assert.Equal(instanceToCreate.Id, resultInstance.Id);
            Assert.Null(resultInstance.InstanceOwnerLookup);
        }


        [Fact]
        public async void InstanceLookupWithNoNumberFailsTest()
        {
            Instance instanceToCreate = new Instance()
            {
                Id = "500004690/5650b227-5f79-41d8-a901-abed492c6fd4",
                AppId = "test/lookup",
                Org = "test"
            };

            Instance instanceTemplate = new Instance()
            {
                InstanceOwnerLookup = new InstanceOwnerLookup()
                {
                }
            };

            PrepareLookupMock(HttpStatusCode.OK, "50004690", instanceToCreate, instanceTemplate);

            ActionResult result = await instanceController.Post("test/appid", null);

            BadRequestObjectResult badResult = result as BadRequestObjectResult;

            Assert.NotNull(badResult);
        }



        [Fact]
        public async void InstanceLookupWithBothPersonAndOrganisationNumberFailsTest()
        {
            Instance instanceToCreate = new Instance()
            {
                Id = "500004690/5650b227-5f79-41d8-a901-abed492c6fd4",
                AppId = "test/lookup",
                Org = "test"
            };

            Instance instanceTemplate = new Instance()
            {
                InstanceOwnerLookup = new InstanceOwnerLookup()
                {
                    PersonNumber = "24",
                    OrganisationNumber = "35"
                }
            };

            PrepareLookupMock(HttpStatusCode.OK, "50004690", instanceToCreate, instanceTemplate);

            ActionResult result = await instanceController.Post("test/appid", null);

            BadRequestObjectResult badResult = result as BadRequestObjectResult;

            Assert.NotNull(badResult);
        }

        private void PrepareLookupMock(HttpStatusCode statusCode, string lookupReturnContent, Instance instanceToCreate, Instance instanceTemplate)
        {
           
            Mock<IInstanceRepository> mockInstanceRepository = new Mock<IInstanceRepository>();
            mockInstanceRepository.Setup(ir => ir.Create(It.IsAny<Instance>())).Returns(Task.FromResult(instanceToCreate));

            Mock<IInstanceEventRepository> mockInstanceEventRepository = new Mock<IInstanceEventRepository>();

            Mock<IApplicationRepository> mockApplicationRepository = new Mock<IApplicationRepository>();
            mockApplicationRepository.Setup(ar => ar.FindOne(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult(new Application()
                {
                    Id = "test/lookup",
                    Org = "test",
                }));
            
            Mock<IDataRepository> mockDataRepository = new Mock<IDataRepository>();
            mockDataRepository.Setup(dr => dr.WriteDataToStorage(It.IsAny<Stream>(), It.IsAny<string>())).Returns(Task.FromResult(50L));

            Mock<ILogger<InstancesController>> mockLogger = new Mock<ILogger<InstancesController>>();
            Mock<IOptions<GeneralSettings>> mockGeneralSettings = new Mock<IOptions<GeneralSettings>>();

            GeneralSettings settings = new GeneralSettings
            {
                BridgeRegisterApiEndpoint = "http://test/"
            };
            mockGeneralSettings.Setup(c => c.Value).Returns(settings);

            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(lookupReturnContent),
                })
                .Verifiable();

            Mock<HttpRequest> request = new Mock<HttpRequest>();
            request.SetupGet(x => x.Headers["Accept"]).Returns("application/json");
            request.SetupGet(x => x.Scheme).Returns("http");
            request.SetupGet(x => x.Host).Returns(new HostString("platform.storage.at21.altinn.cloud"));
            request.SetupGet(x => x.Path).Returns(new PathString("/instances/"));
            request.SetupGet(x => x.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(instanceTemplate))));
            request.SetupGet(x => x.ContentType).Returns("application/json");

            Mock<HttpContext> context = new Mock<HttpContext>();
            context.SetupGet(x => x.Request).Returns(request.Object);

            HttpClient httpClient = new HttpClient(handlerMock.Object);

            instanceController = new InstancesController(
                mockInstanceRepository.Object,
                mockInstanceEventRepository.Object,
                mockApplicationRepository.Object,
                mockDataRepository.Object,
                mockGeneralSettings.Object,
                mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = context.Object,
                },
            };
        }
    }
}
