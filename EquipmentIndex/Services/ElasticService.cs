using EquipmentIndex.ElasticViewModel;
using Nest;

namespace EquipmentIndex.Services
{
    public class ElasticService
    {
        private readonly string _IndexName = "irc-equipmentindex";

        private readonly ElasticClient _elasticClient;

        public ElasticService(ElasticClient client)
        {
            _elasticClient = client;
        }

        public BulkAllObservable<EquipmentElasticViewModel> AddRange(List<EquipmentElasticViewModel> data) =>
                   _elasticClient.BulkAll(data, b => b
                           .Index(_IndexName)
                           .BackOffRetries(200)
                           .BackOffTime("30s")
                           .RefreshOnCompleted()
                           .MaxDegreeOfParallelism(10)
                           .Size(20000)
                   );

        public static TypeMappingDescriptor<EquipmentElasticViewModel> MapEquipmentIndices(TypeMappingDescriptor<EquipmentElasticViewModel> map) => map
                .AutoMap()
                .Properties(ps => ps
                    .Text(t => t
                        .Name(p => p.Name)
                        .Analyzer("EquipmentEnglish-analyzer")
                        .Fields(f => f
                            .Text(p => p
                                .Name("keyword")
                                .Analyzer("EquipmentIndeces-keyword")
                            )
                            .Keyword(p => p
                                .Name("raw")
                            )
                        )
                    )
                    .Text(t => t
                        .Name(p => p.PersianName)
                        .Analyzer("EquipmentPersian-analyzer")
                        .Fields(f => f
                            .Text(p => p
                                .Name("keyword")
                                .Analyzer("EquipmentIndeces-keyword")
                            )
                            .Keyword(p => p
                                .Name("raw")
                            )
                        )
                    )
                    .Text(t => t
                        .Name(p => p.EquipmentId)
                        .Analyzer("EquipmentIndices-analyzer")
                        .Fields(f => f
                            .Text(p => p
                                .Name("keyword")
                                .Analyzer("EquipmentIndeces-keyword")
                            )
                            .Keyword(p => p
                                .Name("raw")
                            )
                        )
                    )
                    .Completion(c => c
                        .Name(p => p.EquipmentSuggest))
                .Keyword(k => k
                .Name(n => n.EquipmentId)
                )
                );

        public static AnalysisDescriptor Analysis(AnalysisDescriptor analysis) => analysis

                .Tokenizers(tokenizers => tokenizers
                .NGram("EquipmentNgram-tokenizer", t => t
                .MinGram(2)
                .MaxGram(3)
                .TokenChars(
                        TokenChar.Letter,
                        TokenChar.Digit,
                        TokenChar.Symbol,
                        TokenChar.Punctuation))
                    .Pattern("EquipmentIndices-pattern", p => p.Pattern(@"\W+"))

                )

                .TokenFilters(tokenFilters => tokenFilters
                .UserDefined("EquipmentNgram-Tokenfilter", new NGramTokenFilter
                {
                    MinGram = 2,
                    MaxGram = 3
                })

                    .WordDelimiter("EquipmentIndices-words", w => w
                        .SplitOnCaseChange()
                        .PreserveOriginal()
                        .SplitOnNumerics()
                        .GenerateNumberParts(false)
                        .GenerateWordParts()

                    )
                )

                .Analyzers(analyzers => analyzers
                    .Custom("EquipmentPersian-analyzer", c => c
                        .Tokenizer("EquipmentNgram-tokenizer")

                        .Filters("EquipmentNgram-Tokenfilter", "lowercase", "decimal_digit", "arabic_normalization", "persian_normalization", "persian_stop")

                    )
                .Custom("EquipmentEnglish-analyzer", c => c
                .Tokenizer("EquipmentNgram-tokenizer")
                .Filters("EquipmentNgram-Tokenfilter", "lowercase", "decimal_digit"))

                    .Custom("EquipmentIndeces-keyword", c => c
                        .Tokenizer("keyword")
                    .Filters("lowercase")

                    )

                 .Custom("EquipmentPersian-keyword", c => c
                        .Tokenizer("keyword")
                    .Filters("lowercase")

                    )
                );
    }
}