using UnityEngine;

public class ControllerModule : Module
{

    override protected void Configure()
    {
        Bind<StateController>();
        Bind<ViewController>();
        Bind<ViewProvider>().In(Scope.PROTOTYPE);
		Bind<NguiTransitionController>();

		Bind<HomeBaseController>().In(Scope.PROTOTYPE);
        Bind<GameController>().In(Scope.PROTOTYPE);
    }
}
