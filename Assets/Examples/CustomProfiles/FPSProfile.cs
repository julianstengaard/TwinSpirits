using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InControl
{
	public class FPSProfile : UnityInputDeviceProfile
	{
		public static string ProfileName = "FPS Keyboard/Mouse";
		public FPSProfile()
		{
			Name = ProfileName;
			Meta = "A keyboard and mouse combination profile appropriate for FPS.";

			SupportedPlatforms = new[]
			{
				"Windows",
				"Mac",
				"Linux"
			};

			Sensitivity = 1.0f;
			LowerDeadZone = 0.0f;

			ButtonMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "Attack",
					Target = InputControlType.RightBumper,
					Source = MouseButton0
				},
				new InputControlMapping
				{
					Handle = "Defend",
					Target = InputControlType.LeftBumper,
					Source = MouseButton1
				},
				new InputControlMapping
				{
					Handle = "SpiritLink",
					Target = InputControlType.LeftBumper,
					Source = KeyCodeButton( KeyCode.Space )
				}
			};

			AnalogMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "Move X",
					Target = InputControlType.LeftStickX,
					Source = KeyCodeAxis( KeyCode.A, KeyCode.D )
				},
				new InputControlMapping
				{
					Handle = "Move Y",
					Target = InputControlType.LeftStickY,
					Source = KeyCodeAxis( KeyCode.S, KeyCode.W )
				},
				new InputControlMapping
				{
					Handle = "Look X",
					Target = InputControlType.RightStickX,
					Source = MouseXAxis,
					Raw    = true
				},
				new InputControlMapping
				{
					Handle = "Look Y",
					Target = InputControlType.RightStickY,
					Source = MouseYAxis,
					Raw    = true
				},
				new InputControlMapping
				{
					Handle = "Look Z",
					Target = InputControlType.ScrollWheel,
					Source = MouseScrollWheel,
					Raw    = true
				}
			};
		}
	}
}

