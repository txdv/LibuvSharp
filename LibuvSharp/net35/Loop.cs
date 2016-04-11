namespace LibuvSharp
{
	public partial class Loop
	{
		/// <summary>
		/// Returns Default Loop value when creating LibuvSharp objects.
		/// </summary>
		/// <value>A loop.</value>
		internal static Loop Constructor {
			get {
				return Loop.Default;
			}
		}
	}
}
