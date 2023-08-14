using Compunet.YoloV8.Plotting;
using Compunet.YoloV8;
using Nukepayload2.Diagnostics;
using System.Diagnostics;
using SixLabors.ImageSharp;
using System.Text.Json;

public readonly record struct TouchInfo(int X, int Y, TimeSpan Duration)
{
    // 可以在这里添加其他属性或方法
}

internal class Program
{
    static string output = "./assets/output";
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Task.Delay(5000).Wait();

        #region 触屏测试
        /*
        Task.Run(() =>
        {
            //return;
            int posX = 350;
            int posY = 350;
            int dragToX = posX + 100;
            int dragToY = posY + 20;
            DateTime now = DateTime.Now;
            while (DateTime.Now - now < TimeSpan.FromSeconds(10))
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
            int durationMilliseconds = 100;
            Queue<TouchInfo> dragToPositions = new();
            for (int x = 0; x < 5; x++)
            {
                dragToPositions.Enqueue(new(dragToX, dragToY, TimeSpan.FromMilliseconds(durationMilliseconds)));
                dragToX = Random.Shared.Next(300, 800);
                dragToY = Random.Shared.Next(300, 800);
                durationMilliseconds = Random.Shared.Next(150, 2000);
            }

            while (true)
            {
                // 检查上次调用是否还没有结束
                if (!InteractionEx.IsSendTouchFinished())
                {
                    // 上次调用还没有结束，直接修改参数
                    for (int x = 0; x < 5; x++)
                    {
                        dragToPositions.Enqueue(new(dragToX, dragToY, TimeSpan.FromMilliseconds(durationMilliseconds)));
                        dragToX = Random.Shared.Next(300, 800);
                        dragToY = Random.Shared.Next(300, 800);
                        durationMilliseconds = Random.Shared.Next(150, 2000);
                    }
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss:fffffff}\tIsSendTouchFinished");
                }
                else
                {
                    // 上次调用已经结束，调用 SendTouch 方法
                    //return;
                    Task.Run(() =>
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss:fffffff}\t{nameof(posX)}:{posX}\t{nameof(posY)}:{posY}\t{nameof(dragToX)}:{dragToX}\t{nameof(dragToY)}:{dragToY}");

                        InteractionEx.SendTouch(
                            posX: posX,
                            posY: posY,
                            dragToPositions: ref dragToPositions,
                            durationMilliseconds: (int)TimeSpan.FromSeconds(1).TotalMilliseconds,
                            //orientation: 0,
                            //pointerId: 0,
                            pressure: Random.Shared.Next(0, 1024),
                            //areaSize: (48, 48),
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
                }

                Task.Delay(TimeSpan.FromSeconds(1 * dragToPositions.Count * 0.8)).Wait();
            }
        });
        */
        #endregion
        //InputInjector input = InputInjector.TryCreateWithPreviewFeatures();
        //input.InitializeTouchInjection(InjectedInputVisualizationMode.Default, maxCount: 10);
        //input.InjectTouchInput(new InjectedInputTouchInfo() { })

        string img = @"C:\Users\huang\OneDrive\Pictures\bili\1674347857914.jpg";

        if (Directory.Exists(output) == false)
            Directory.CreateDirectory(output);

        PoseDemo(img, "./assets/models/yolov8s-pose.onnx");
        DetectDemo(img, "./assets/models/yolov8s.onnx");
        SegmentDemo(img, "./assets/models/yolov8s-seg.onnx");

        while (true)
        {
            var r = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(r) && r.ToUpper() == "EXIT") break;
        }
    }

    static void PoseDemo(string image, string model)
    {
        Console.WriteLine();
        Console.WriteLine("================ POSE DEMO ================");
        Console.WriteLine();

        Console.WriteLine("Loading model...");
        using var predictor = new YoloV8(model);

        Console.WriteLine("Working...");
        var result = predictor.Pose(image);

        Console.WriteLine();

        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Speed: {result.Speed}");

        Console.WriteLine();

        Console.WriteLine("Plotting and saving...");
        using var origin = Image.Load(image);

        using var ploted = result.PlotImage(origin);

        var pathToSave = Path.Combine(output, Path.GetFileName(image));

        ploted.Save(pathToSave);
    }

    static void DetectDemo(string image, string model)
    {
        Console.WriteLine();
        Console.WriteLine("================ DETECTION DEMO ================");
        Console.WriteLine();

        Console.WriteLine("Loading model...");
        using var predictor = new YoloV8(model);

        Console.WriteLine("Working...");
        var result = predictor.Detect(image);

        Console.WriteLine();

        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Speed: {result.Speed}");

        Console.WriteLine();

        Console.WriteLine("Plotting and saving...");
        using var origin = Image.Load(image);

        using var ploted = result.PlotImage(origin);

        var pathToSave = Path.Combine(output, Path.GetFileName(image));

        ploted.Save(pathToSave);
    }

    static void SegmentDemo(string image, string model)
    {
        Console.WriteLine();
        Console.WriteLine("================ SEGMENTATION DEMO ================");
        Console.WriteLine();

        Console.WriteLine("Loading model...");
        using var predictor = new YoloV8(model);

        Console.WriteLine("Working...");
        var result = predictor.Segment(image);

        Console.WriteLine();

        Console.WriteLine($"Result: {System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions() { IncludeFields = true })}");
        Console.WriteLine($"Speed: {result.Speed}");

        Console.WriteLine();

        Console.WriteLine("Plotting and saving...");
        using var origin = Image.Load(image);

        using var ploted = result.PlotImage(origin, new SegmentationPlottingOptions { MaskConfidence = .65F });

        var filename = $"{Path.GetFileNameWithoutExtension(image)}_seg";
        var extension = Path.GetExtension(image);

        var pathToSave = Path.Combine(output, filename + extension);

        ploted.Save(pathToSave);
    }
}

public static class InteractionEx
{
    internal static readonly InputInjector s_injection = null!;
    private static bool IsSendTouchRunning { get; set; } = false;
    private static object LockObject { get; set; } = new();

    static InteractionEx()
    {
        s_injection = InputInjector.TryCreate();
        s_injection?.InitializeTouchInjection(InjectedInputVisualizationMode.Default);
    }

    public static void SendTouch(
        int posX,
        int posY,
        ref Queue<TouchInfo>? dragToPositions,
        int durationMilliseconds = 150,
        int orientation = 90,
        int pointerId = 0,
        int pressure = 1024,
        (double Width, double Height)? areaSize = null,
        //(int X, int Y)? dragTo = null,
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

            // 设置标志变量为正在运行状态
            SetSendTouchRunning(true);

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

                    //foreach (var dragTo in dragToPositions)
                    while (dragToPositions.Any())
                    {
                        var drag = dragToPositions.Dequeue();
                        int num3 = drag.X;
                        int num4 = drag.Y;
                        double elapsedMilliseconds = 0;
                        while (elapsedMilliseconds < drag.Duration.TotalMilliseconds)
                        {
                            double arg = elapsedMilliseconds / drag.Duration.TotalMilliseconds;
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

            // 设置标志变量为已结束状态
            SetSendTouchRunning(false);
        }
    }

    public static bool IsSendTouchFinished()
    {
        lock (LockObject)
        {
            return !IsSendTouchRunning;
        }
    }

    private static void SetSendTouchRunning(bool running)
    {
        lock (LockObject)
        {
            IsSendTouchRunning = running;
        }
    }
}