namespace DomainEntity.Common
{
    /// <summary>
    /// 目前等同於 BaseGridRespModel
    /// </summary>
    /// <typeparam name="GM"></typeparam>
    public class BaseGridRespDTO<GM>
    {
        public IEnumerable<GM> GridRespResult { get; set; } = new List<GM>();

        public int GridTotalCount { get; set; } = 0;
    }
}
