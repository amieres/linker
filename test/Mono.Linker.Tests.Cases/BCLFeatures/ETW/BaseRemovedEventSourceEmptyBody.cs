using System.Diagnostics.Tracing;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;

namespace Mono.Linker.Tests.Cases.BCLFeatures.ETW
{
#if NETCOREAPP
	[IgnoreTestCase ("--exclude-feature is not supported on .NET Core")]
#endif
	[SetupLinkerArgument ("--exclude-feature", "etw")]
	// Keep framework code that calls EventSource methods like OnEventCommand
	[SetupLinkerCoreAction ("skip")]
	public class BaseRemovedEventSourceEmptyBody
	{
		public static void Main ()
		{
			var b = CustomCtorEventSourceEmptyBody.Log.IsEnabled ();
			if (b)
				CustomCtorEventSourceEmptyBody.Log.SomeMethod ();
		}
	}

	[Kept]
	[KeptBaseType (typeof (EventSource))]
	[KeptMember (".ctor()")]
	[KeptMember (".cctor()")]
	[EventSource (Name = "MyCompany")]
	class CustomCtorEventSourceEmptyBody : EventSource
	{
		public class Keywords
		{
			public const EventKeywords Page = (EventKeywords) 1;

			public int Unused;
		}

		[Kept]
		public static CustomCtorEventSourceEmptyBody Log = new MyEventSourceBasedOnCustomCtorEventSourceEmptyBody (1);

		[Kept]
		[ExpectedInstructionSequence (new[]
		{
			"ldarg.0",
			"call",
			"ret",
		})]
		public CustomCtorEventSourceEmptyBody (int value)
		{
			Removed ();
		}

		[Kept]
		[ExpectedInstructionSequence (new[]
		{
			"ret"
		})]
		protected override void OnEventCommand (EventCommandEventArgs command)
		{
			// A nop is removed
		}

		[Kept]
		[ExpectedInstructionSequence (new[]
		{
			"ret"
		})]
		[Event (8)]
		public void SomeMethod ()
		{
			Removed ();
		}

		public void Removed ()
		{
		}
	}

	[Kept]
	[KeptBaseType (typeof (CustomCtorEventSourceEmptyBody))]
	class MyEventSourceBasedOnCustomCtorEventSourceEmptyBody : CustomCtorEventSourceEmptyBody
	{
		[Kept]
		public MyEventSourceBasedOnCustomCtorEventSourceEmptyBody (int value) : base (value)
		{
		}
	}
}