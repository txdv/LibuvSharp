using System;

using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class ComputerFixture
	{
		[Test]
		public void Base()
		{
			Assert.Greater(Computer.HighResolutionTime, 0);
			Assert.Greater(Computer.Uptime, 0);
		}

		[Test]
		public void CpuInfo()
		{
			Assert.NotNull(Computer.CpuInfo);
			Assert.Greater(Computer.CpuInfo.Length, 0);
			foreach (var cpu in Computer.CpuInfo) {
				Assert.NotNull(cpu);
				Assert.NotNull(cpu.Name);
/*
				Assert.Greater(cpu.Speed);
				Assert.Greater(cpu.Times.Idle,   0);
				Assert.Greater(cpu.Times.Nice,   0);
				Assert.Greater(cpu.Times.System, 0);
				Assert.Greater(cpu.Times.User,   0);
*/
			}
		}

		[Test]
		public void MemoryInfo()
		{
			Assert.Greater(Computer.Memory.Free, 0);
			Assert.Greater(Computer.Memory.Total, 0);
			Assert.Greater(Computer.Memory.Used, 0);
		}

		[Test]
		public void Load()
		{
			Assert.Greater(Computer.Load.Last,    0);
			Assert.Greater(Computer.Load.Five,    0);
			Assert.Greater(Computer.Load.Fifteen, 0);
		}

		[Test]
		public void NetworkInfo()
		{
			Assert.NotNull(Computer.NetworkInterfaces);
			foreach (var nif in Computer.NetworkInterfaces) {
				Assert.NotNull(nif.Name);
				Assert.NotNull(nif.Address);
			}
		}
	}
}

