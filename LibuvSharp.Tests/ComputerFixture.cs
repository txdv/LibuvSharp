using System;

using Xunit;

namespace LibuvSharp.Tests
{
	public class ComputerFixture
	{
		[Fact]
		public void Base()
		{
			Assert.True(Computer.HighResolutionTime > 0);
			Assert.True(Computer.Uptime > 0);
		}

		[Fact]
		public void CpuInfo()
		{
			Assert.NotNull(Computer.CpuInfo);
			Assert.True(Computer.CpuInfo.Length > 0);
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

		[Fact]
		public void MemoryInfo()
		{
			Assert.True(Computer.Memory.Free > 0);
			Assert.True(Computer.Memory.Total > 0);
			Assert.True(Computer.Memory.Used > 0);
		}

		[Fact]
		public void Load()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				Assert.True(Computer.Load.Last    > 0);
				Assert.True(Computer.Load.Five    > 0);
				Assert.True(Computer.Load.Fifteen > 0);
			}
		}

		[Fact]
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

