using System.Dynamic;
using System.Text.RegularExpressions;

namespace SimulationOfDevices.Services.RabbitMQ
{
    public class CreateSettingsForSendFeed
    {
        public static FeedSettings Create(DeviceSettings deviceSettings)
        {
            var publishDurationFromSettings = GetPublishRateOrDurationFromSettings(deviceSettings.JobParamaterModel.Duration);
            var publishRateFromSettings = GetPublishRateOrDurationFromSettings(deviceSettings.JobParamaterModel.PublishRate);
            var functionForSendingData = GetFunctionFromSettings(deviceSettings.JobParamaterModel.Function);

            TimeSpan period = CreatePublishRateOdDurationTimeSpan(publishRateFromSettings);            
            TimeSpan durationTimeForSendingData = CreatePublishRateOdDurationTimeSpan(publishDurationFromSettings);
            

            return new FeedSettings
            {
                Reference = deviceSettings.Reference,
                PublishRate = period,
                PublishDuration = durationTimeForSendingData,
                Function = functionForSendingData,
            };
        }

        public static TimeSpan PublishRateOrDurationFromSettings(string inputString)
        {
            var publishRateOrDurationFromSettings = GetPublishRateOrDurationFromSettings(inputString);
            return CreatePublishRateOdDurationTimeSpan(publishRateOrDurationFromSettings);
        }

        private static TimeSpan CreatePublishRateOdDurationTimeSpan(PublishRateOrDurationFromSettings publishRateFromSettings)
        {
            var result = TimeSpan.Zero;
            switch (publishRateFromSettings.Unit)
            {
                case TimeUnit.s:
                    result = TimeSpan.FromSeconds(publishRateFromSettings.Value);
                    break;
                case TimeUnit.m:
                    result = TimeSpan.FromMinutes(publishRateFromSettings.Value);
                    break;
                case TimeUnit.h:
                    result = TimeSpan.FromHours(publishRateFromSettings.Value);
                    break;
                case TimeUnit.d:
                    result = TimeSpan.FromDays(publishRateFromSettings.Value);
                    break;
                default:
                    break;
            }
            return result;
        }

        public static List<ExpandoObject> CreateRandomValues(string reference, FunctionFromSettings functionFromSettings)
        {
            double valueForSend = 0.0;
            switch (functionFromSettings.FunctionForSendingData)
            {
                case FunctionForSendingData.Variate:
                    valueForSend = Variate(functionFromSettings.Value, functionFromSettings.ValueOrPrecentage);
                    break;
                case FunctionForSendingData.Random:
                    valueForSend = GetRandomValueBetween(functionFromSettings.Value, functionFromSettings.ValueOrPrecentage);
                    break;
                case FunctionForSendingData.Fixed:
                    valueForSend = functionFromSettings.Value;
                    break;
                case FunctionForSendingData.SKIP:
                    valueForSend = 0.0;
                    break;
                default:
                    break;
            }

            var dynamicObjectList = new List<ExpandoObject>();

            var dynamicObject = new ExpandoObject();
            dynamicObject.TryAdd(reference, valueForSend);

            dynamicObjectList.Add(dynamicObject);
            return dynamicObjectList;
        }

        private static FunctionFromSettings GetFunctionFromSettings(string functionFromSettingsJson)
        {
            if (functionFromSettingsJson.Equals("SKIP"))
            {
                return new FunctionFromSettings()
                {
                    FunctionForSendingData = FunctionForSendingData.SKIP,
                    Value = 0.0,
                    ValueOrPrecentage = 0.0
                };
            }
           
            // Use a regular expression to match the string before the parentheses and the values between the parentheses
            Match match = Regex.Match(functionFromSettingsJson, @"(.*)\(([^)]*)\)");
            string functionName = match.Groups[1].Value; // Variate           
            string values = match.Groups[2].Value; // 140,5

            if (functionName.Equals("Fixed"))
            {
                return new FunctionFromSettings()
                {
                    FunctionForSendingData = FunctionForSendingData.Fixed,
                    Value = double.Parse(values),
                    ValueOrPrecentage = 0.0
                };
            }

            // Split the values on the comma and parse them as doubles
            string[] valueStrings = values.Split(',');
            double value = double.Parse(valueStrings[0]); // 140
            double valueOrPrecentage = double.Parse(valueStrings[1]); // 5

            FunctionForSendingData functionFofSendingData;
            Enum.TryParse(functionName, true, out functionFofSendingData);

            return new FunctionFromSettings()
            {
                FunctionForSendingData = functionFofSendingData,
                Value = value,
                ValueOrPrecentage = valueOrPrecentage
            };
        }

        private static PublishRateOrDurationFromSettings GetPublishRateOrDurationFromSettings(string inputString)
        {
            //"15s"

            Match match = Regex.Match(inputString, @"(\d+)([shm])");
            double value = double.Parse(match.Groups[1].Value); // 15
            string unit = match.Groups[2].Value; // s

            TimeUnit unitForPublishing;
            Enum.TryParse(unit, true, out unitForPublishing);

            return new PublishRateOrDurationFromSettings()
            {
                Value = value,
                Unit = unitForPublishing
            };
        }

        private static double Variate(double value, double percentage)
        {
            double minValue = value * (1 - percentage / 100);
            double maxValue = value * (1 + percentage / 100);

            return GetRandomValueBetween(minValue, maxValue);
        }

        private static double GetRandomValueBetween(double minValue, double maxValue)
        {
            // Generate a random number between the minimum and maximum values
            Random random = new Random();
#pragma warning disable CA5394 // Do not use insecure randomness
            return random.NextDouble() * (maxValue - minValue) + minValue;
#pragma warning restore CA5394 // Do not use insecure randomness
        }        
    }

    public sealed class PublishRateOrDurationFromSettings
    {
        public double Value { get; set; }
        public TimeUnit Unit { get; set; }
    }
    public sealed class FunctionFromSettings
    {
        public FunctionForSendingData FunctionForSendingData { get; set; }
        public double Value { get; set; }
        public double ValueOrPrecentage { get; set; }
    }

    public enum FunctionForSendingData
    {
        Variate,
        Fixed,
        Random,
        SKIP
    }

    public enum TimeUnit
    {
        s,
        m,
        h,
        d
    }
}