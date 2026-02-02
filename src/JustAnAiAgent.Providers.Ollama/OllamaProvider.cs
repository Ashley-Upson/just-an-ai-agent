using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JustAnAiAgent.Providers.Ollama.Services.Foundation;

namespace JustAnAiAgent.Providers.Ollama;

public class OllamaProvider(OllamaService service)
{
    private readonly OllamaService service;
    private OllamaClient client;

}