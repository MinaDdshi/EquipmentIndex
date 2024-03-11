using AutoMapper;
using EquipmentIndex.Database;
using EquipmentIndex.ElasticViewModel;
using EquipmentIndex.Services;
using Nest;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

var elasticClient = new ElasticClient(
        new ConnectionSettings(new Uri("https://elastic.ttac.ir/"))
        .BasicAuthentication("mohamadali.ebrahimzade","ITSmemohammad021098@")
        .DisableDirectStreaming()
        .DefaultMappingFor<EquipmentElasticViewModel>(i=>i));

elasticClient.Indices.Create("irc-equipmentindex", x => x
.Settings(s => s
.NumberOfShards(1)
.NumberOfReplicas(2)
.Analysis(ElasticService.Analysis))
.Map<EquipmentElasticViewModel>(ElasticService.MapEquipmentIndices));

var service = new ElasticService(elasticClient);




var db = new EquipmentContext();

var allEquipments = db!.EquipmentIndexes!.ToList();
Console.WriteLine("count:"+ allEquipments.Count);
Console.WriteLine("***********************");

var counter = 0;

var ResultBag = new List<EquipmentElasticViewModel>();
var conbag = new ConcurrentBag<EquipmentElasticViewModel>();
var time = Stopwatch.StartNew();
time.Start();
foreach (var item in allEquipments)
{
        var config = new MapperConfiguration(c => c.CreateMap<EquipmentIndexes, EquipmentElasticViewModel>()
        .ForMember(ex => ex.EquipmentSuggest, opt => opt.Ignore()));

        IMapper mapper = config.CreateMapper();
        var model = mapper.Map<EquipmentElasticViewModel>(item);
        model.EquipmentSuggest = new CompletionField
        {
                Input = new List<string>
                {
                        item.EquipmentId.ToString() ??string.Empty,
                        item.Name ?? string.Empty,
                        item.PersianName ?? string.Empty,
                }
        };

        ResultBag!.Add(model);
        counter++;
        Console.WriteLine($"{counter} Added From {allEquipments.Count}");
}

//Parallel.ForEach(AllEquipments, new ParallelOptions { MaxDegreeOfParallelism = 200 }, item =>
//{

//        var config = new MapperConfiguration(c => c.CreateMap<EquipmentIndexes, EquipmentElasticViewModel>()
//        .ForMember(ex => ex.EquipmentSuggest, opt => opt.Ignore()));

//        IMapper mapper = config.CreateMapper();
//        var model = mapper.Map<EquipmentElasticViewModel>(item);
//        model.ResultType = "تجهیزات پزشکی";
//        model.EquipmentSuggest = new CompletionField
//        {
//                Input = new List<string>
//                {
//                        item.EquipmentId.ToString() ??string.Empty,
//                        item.Name ?? string.Empty,
//                        item.PersianName ?? string.Empty,
//                }
//        };

//        conbag!.Add(model);
//        counter++;
//        Console.WriteLine($"{counter} Added From {AllEquipments.Count}");


//});
//var filan = conbag.ToList();

var bulk = service.AddRange(ResultBag);

var waitHandle = new CountdownEvent(1);

ExceptionDispatchInfo? captureInfo = null;

bulk.Subscribe(new BulkAllObserver(
        onNext: b =>
        {
        },
        onError: e =>
        {
                captureInfo = ExceptionDispatchInfo.Capture(e);
                waitHandle.Signal();
        },
        onCompleted: () => waitHandle.Signal()
));

waitHandle.Wait(TimeSpan.FromMinutes(5));
captureInfo?.Throw();
time.Stop();
Console.WriteLine(time?.ElapsedMilliseconds);
Console.WriteLine("All Jobs Done !");

Console.ReadKey(false);