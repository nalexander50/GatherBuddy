﻿using System.Numerics;
using GatherBuddy.Utility;
using ImGuiNET;

namespace GatherBuddy.Gui
{
    public partial class Interface
    {
        private void DrawRecordBox()
            => DrawCheckbox("Record",
                "Toggle whether to record all encountered nodes in regular intervals.\n"
              + "Recorded node coordinates are more accurate than pre-programmed ones and will be used for map markers and aetherytes instead.\n"
              + "Records are saved in compressed form in the plugin configuration.",
                GatherBuddy.Config.DoRecord, b =>
                {
                    if (b)
                        _plugin.Gatherer!.StartRecording();
                    else
                        _plugin.Gatherer!.StopRecording();
                    GatherBuddy.Config.DoRecord = b;
                });

        private void DrawAlarmToggle()
            => DrawCheckbox("Alarms",             "Toggle all alarms on or off.",
                GatherBuddy.Config.AlarmsEnabled, b =>
                {
                    if (b)
                        _plugin.Alarms!.Enable();
                    else
                        _plugin.Alarms!.Disable();
                    GatherBuddy.Config.AlarmsEnabled = b;
                });

        private void DrawSnapshotButton()
        {
            if (ImGui.Button("Snapshot", new Vector2(-1, 0)))
                Dalamud.Chat.Print($"Recorded {_plugin.Gatherer!.Snapshot()} new nearby gathering nodes.");
            ImGuiHelper.HoverTooltip("Record currently available nodes around you once.");
        }

        private void DrawHeaderRow()
        {
            var spacing = 5 * _horizontalSpace;
            using (var _ = ImGuiRaii.NewGroup())
            {
                DrawAlarmToggle();
                HorizontalSpace(spacing);
                DrawRecordBox();
                HorizontalSpace(spacing);
                DrawSnapshotButton();
            }

            ImGui.Dummy(new Vector2(0, _horizontalSpace / 2));
        }

        private static void DrawButtonText(string text, Vector2 size, uint color)
        {
            using var imgui = new ImGuiRaii()
                .PushColor(ImGuiCol.Button,        color)
                .PushColor(ImGuiCol.ButtonHovered, color)
                .PushColor(ImGuiCol.ButtonActive,  color);

            ImGui.Button(text, size);
        }

        private static void DrawEorzeaTime(string time)
            => DrawButtonText(time, new Vector2(0, _weatherIconSize.Y), Colors.HeaderRow.EorzeaTime);

        private static void DrawNextEorzeaHour(string hour, Vector2 size)
            => DrawButtonText(hour, size, Colors.HeaderRow.NextEorzeaHour);

        private void DrawNextWeather(string nextWeather)
        {
            if (_headerCache.Territory != 0)
            {
                ImGui.SameLine();
                ImGui.Image(_headerCache.CurrentWeather!.ImGuiHandle, _weatherIconSize);
                ImGui.SameLine();
                DrawButtonText(nextWeather, new Vector2(0, _weatherIconSize.Y), Colors.HeaderRow.Weather);
                ImGui.SameLine();
                ImGui.Image(_headerCache.NextWeather!.ImGuiHandle, _weatherIconSize);
            }
            else
            {
                ImGui.SameLine();
                ImGui.Dummy(_weatherIconSize);
                ImGui.SameLine();
                DrawButtonText(nextWeather, new Vector2(0, _weatherIconSize.Y), Colors.HeaderRow.Weather);
                ImGui.SameLine();
                ImGui.Dummy(_weatherIconSize);
            }
        }

        private void DrawTimeRow(long eorzeaHour, long eorzeaMinute)
        {
            var minuteOfHour = eorzeaMinute % RealTime.MinutesPerHour;
            var hourOfDay    = eorzeaHour % RealTime.HoursPerDay;

            var nextHourS = EorzeaTime.SecondsPerEorzeaHour - RealTime.CurrentTimestamp() % EorzeaTime.SecondsPerEorzeaHour;
            var nextHourM = nextHourS / RealTime.SecondsPerMinute;

            var nextWeatherS = nextHourS + EorzeaTime.SecondsPerEorzeaHour * (7 - hourOfDay % 8);
            var nextWeatherM = nextWeatherS / RealTime.SecondsPerMinute;

            nextHourS    -= nextHourM * RealTime.SecondsPerMinute;
            nextWeatherS -= nextWeatherM * RealTime.SecondsPerMinute;

            var nextWeatherString = $"  {nextWeatherM:D2}.{nextWeatherS:D2} Min.  ";
            var width = -(ImGui.CalcTextSize(nextWeatherString).X
              + (_weatherIconSize.X + _itemSpacing.X * 1.5f + ImGui.GetStyle().FramePadding.X) * 2);

            var territory = Dalamud.ClientState.TerritoryType;
            _headerCache.Update(eorzeaHour, territory);

            DrawEorzeaTime($"ET {hourOfDay:D2}:{minuteOfHour:D2}");
            ImGui.SameLine();
            DrawNextEorzeaHour($"{nextHourM:D2}.{nextHourS:D2} Min to next Eorzea hour.", new Vector2(width, _weatherIconSize.Y));
            ImGui.SameLine();
            DrawNextWeather(nextWeatherString);

            ImGui.Dummy(new Vector2(0, _horizontalSpace / 2));
        }
    }
}
