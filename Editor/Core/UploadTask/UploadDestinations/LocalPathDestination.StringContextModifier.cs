namespace Wireframe
{
    public partial class LocalPathDestination
    {
        protected override Context CreateContext()
        {
            Context context = base.CreateContext();
            context.AddCommand(Context.DESTINATION_LOCAL_PATH_KEY, FullPath);
            return context;
        }
    }
}