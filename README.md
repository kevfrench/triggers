# triggers

[![forthebadge](https://forthebadge.com/images/badges/60-percent-of-the-time-works-every-time.svg)](https://forthebadge.com)

A simple example for me to play with testing time-based Observables using the TestScheduler provided in [Microsoft.Reactive.Testing](https://www.nuget.org/packages/Microsoft.Reactive.Testing/).

So I had something to test I bashed together a simple trigger provider. When I say trigger, I mean an *IObservable&lt;Unit&gt;* that emits on a fixed frequency. Basically the provider contains a single  *Observable.Interval* that emits a *Unit* at 50Hz. It uses *Observable.Buffer* to reduce the frequency as desired.