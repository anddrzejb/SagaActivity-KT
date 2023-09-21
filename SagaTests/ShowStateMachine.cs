using Automatonymous.Graphing;
using Automatonymous.Visualizer;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Saga.Configuration;
using Xunit.Abstractions;

namespace SagaTests;

public class ShowStateMachine
{
    private readonly ITestOutputHelper output;

    public ShowStateMachine(ITestOutputHelper output)
    {
        this.output = output;
    }

    /// <summary>
    /// Visualize graph using for example http://viz-js.com
    /// </summary>
    [Fact]
    public void Show_me_the_state_machine()
    {
        var logger = Substitute.For<ILogger<SagaStateMachine>>();
        var orderStateMachine = new SagaStateMachine(logger);
        var graph = orderStateMachine.GetGraph();

        var generator = new StateMachineGraphvizGenerator(graph);

        var dots = generator.CreateDotFile();
        this.output.WriteLine(dots);
    }    
}