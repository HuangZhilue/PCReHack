using Nukepayload2.Diagnostics;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Task.Delay(5000).Wait();

        Task.Run(() =>
        {
            //return;
            int posX = 350;
            int posY = 350;
            int dragToX = posX + 100;
            int dragToY = posY + 20;
            DateTime now = DateTime.Now;
            while (DateTime.Now - now < TimeSpan.FromSeconds(5))
            {
                int durationMilliseconds = (int)((Random.Shared.Next(10, 100) / 100d) * 1000d);
                Console.WriteLine($"{DateTime.Now:HH:mm:ss:fffffff}\t{nameof(durationMilliseconds)}:{durationMilliseconds}\t{nameof(posX)}:{posX}\t{nameof(posY)}:{posY}\t{nameof(dragToX)}:{dragToX}\t{nameof(dragToY)}:{dragToY}");

                Interaction.SendTouch(
                    posX: posX,
                    posY: posY,
                    durationMilliseconds: durationMilliseconds,
                    //orientation: 0,
                    //pointerId: 0,
                    pressure: Random.Shared.Next(0, 1024),
                    //areaSize: (48, 48),
                    dragTo: (dragToX, dragToY),
                    dragEasingX: (b) =>
                    {
                        return b;
                    },
                    dragEasingY: (b) =>
                    {
                        return b;
                    }
                    );
                posX = dragToX;
                posY = dragToY;
                dragToX = Random.Shared.Next(300, 800);
                dragToY = Random.Shared.Next(300, 800);
            }
        });

        Task.Run(() =>
        {
            return;
            int posX = 350;
            int posY = 350;
            int dragToX = posX;
            int dragToY = posY;
            List<(int X, int Y)> dragToPositions = new();
            for (int x = 0; x < 50; x++)
            {
                dragToPositions.Add((dragToX, dragToY));
                dragToX = Random.Shared.Next(300, 800);
                dragToY = Random.Shared.Next(300, 800);
            }

            Console.WriteLine($"{DateTime.Now:HH:mm:ss:fffffff}\t{nameof(posX)}:{posX}\t{nameof(posY)}:{posY}\t{nameof(dragToX)}:{dragToX}\t{nameof(dragToY)}:{dragToY}");

            InteractionEx.SendTouch(
                posX: posX,
                posY: posY,
                durationMilliseconds: (int)TimeSpan.FromSeconds(10).TotalMilliseconds,
                //orientation: 0,
                //pointerId: 0,
                pressure: Random.Shared.Next(0, 1024),
                //areaSize: (48, 48),
                dragToPositions: dragToPositions,
                dragEasingX: (b) =>
                {
                    return b;
                },
                dragEasingY: (b) =>
                {
                    return b;
                }
                );

            Console.WriteLine($"{DateTime.Now:HH:mm:ss:fffffff}\t{nameof(posX)}:{posX}\t{nameof(posY)}:{posY}\t{nameof(dragToX)}:{dragToX}\t{nameof(dragToY)}:{dragToY}");

        });
        //InputInjector input = InputInjector.TryCreateWithPreviewFeatures();
        //input.InitializeTouchInjection(InjectedInputVisualizationMode.Default, maxCount: 10);
        //input.InjectTouchInput(new InjectedInputTouchInfo() { })

        while (true)
        {
            var r = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(r) && r.ToUpper() == "EXIT") break;
        }
    }
}

public static class InteractionEx
{
    internal static readonly InputInjector s_injection = null!;

    static InteractionEx()
    {
        s_injection = InputInjector.TryCreate();
        s_injection?.InitializeTouchInjection(InjectedInputVisualizationMode.Default);
    }

    public static void SendTouch(
        int posX,
        int posY,
        int durationMilliseconds = 150,
        int orientation = 90,
        int pointerId = 0,
        int pressure = 1024,
        (double Width, double Height)? areaSize = null,
        //(int X, int Y)? dragTo = null,
        List<(int X, int Y)>? dragToPositions = null,
        Func<double, double>? dragEasingX = null,
        Func<double, double>? dragEasingY = null)
    {
        if (durationMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(durationMilliseconds), "Value can't be < 0.");
        if (orientation < 0 || orientation > 359) throw new ArgumentOutOfRangeException(nameof(orientation), "Value can't be < 0 or > 359.");
        if (pressure < 0 || pressure > 1024) throw new ArgumentOutOfRangeException(nameof(pressure), "Value can't be < 0 or > 1024.");


        checked
        {
            int num;
            int num2;
            if (!areaSize.HasValue)
            {
                num = 2;
                num2 = 2;
            }
            else
            {
                (double Width, double Height) = areaSize.Value;
                if (Width <= 0.0)
                {
                    throw new ArgumentOutOfRangeException("sizeValue", "Width can't be <= 0.");
                }

                if (Height <= 0.0)
                {
                    throw new ArgumentOutOfRangeException("sizeValue", "Height can't be <= 0.");
                }

                num = (int)Math.Round(Width / 2.0);
                num2 = (int)Math.Round(Height / 2.0);
            }

            if (s_injection == null)
            {
                throw new PlatformNotSupportedException();
            }

            InputInjector inputInjector = s_injection;
            InjectedInputTouchInfo injectedInputTouchInfo = default;
            injectedInputTouchInfo.PointerInfo = new InjectedInputPointerInfo
            {
                PointerId = (uint)pointerId,
                PixelLocation = new InjectedInputPoint
                {
                    PositionX = posX,
                    PositionY = posY
                },
                PointerType = PointerInputType.Touch
            };
            injectedInputTouchInfo.TouchParameters = InjectedInputTouchParameters.Contact | InjectedInputTouchParameters.Orientation | InjectedInputTouchParameters.Pressure;
            injectedInputTouchInfo.Orientation = (uint)orientation;
            injectedInputTouchInfo.Pressure = (uint)pressure;
            InjectedInputPoint pixelLocation = injectedInputTouchInfo.PointerInfo.PixelLocation;
            injectedInputTouchInfo.Contact = new InjectedInputRectangle
            {
                Top = pixelLocation.PositionY - num2,
                Bottom = pixelLocation.PositionY + num2,
                Left = pixelLocation.PositionX - num,
                Right = pixelLocation.PositionX + num
            };
            injectedInputTouchInfo.PointerInfo.PointerOptions = (InjectedInputPointerOptions)65542u;
            inputInjector.InjectTouchInput(injectedInputTouchInfo);
            injectedInputTouchInfo.PointerInfo.PointerOptions = (InjectedInputPointerOptions)131078u;
            if (durationMilliseconds > 0)
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();
                SpinWait spinWait = default;
                if (dragToPositions is null || dragToPositions.Count ==0)
                {
                    while (stopwatch.ElapsedMilliseconds < durationMilliseconds)
                    {
                        inputInjector.InjectTouchInput(injectedInputTouchInfo);
                        spinWait.SpinOnce();
                    }
                }
                else
                {
                    dragEasingX ??= (double d) => d;

                    dragEasingY ??= (double d) => d;

                    foreach (var dragTo in dragToPositions)
                    {
                        (int num3, int num4) = dragTo;
                        double elapsedMilliseconds = 0;
                        while (elapsedMilliseconds < durationMilliseconds / dragToPositions.Count)
                        {
                            double arg = elapsedMilliseconds / (durationMilliseconds / dragToPositions.Count);
                            int positionX = posX + (int)Math.Round((num3 - posX) * dragEasingX(arg));
                            int positionY = posY + (int)Math.Round((num4 - posY) * dragEasingY(arg));
                            injectedInputTouchInfo.PointerInfo.PixelLocation.PositionX = positionX;
                            injectedInputTouchInfo.PointerInfo.PixelLocation.PositionY = positionY;
                            inputInjector.InjectTouchInput(injectedInputTouchInfo);
                            spinWait.SpinOnce();
                            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                        }

                        posX = num3;
                        posY = num4;
                        stopwatch.Restart();
                    }
                }

                stopwatch.Stop();
            }

            injectedInputTouchInfo.PointerInfo.PointerOptions = InjectedInputPointerOptions.PointerUp;
            inputInjector.InjectTouchInput(injectedInputTouchInfo);
            inputInjector = null!;
        }
    }
}