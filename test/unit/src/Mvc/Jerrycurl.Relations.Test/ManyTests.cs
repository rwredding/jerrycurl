using Shouldly;

namespace Jerrycurl.Relations.Tests
{
    public class ManyTests
    {
        public void Test_Many_Equality()
        {
            Many<int> empty = new Many<int>();
            Many<int> zero = new Many<int>(0);
            Many<int> one = new Many<int>(1);

            Many<int> empty2 = new Many<int>();
            Many<int> zero2 = new Many<int>(0);
            Many<int> one2 = new Many<int>(1);

            empty.Equals(empty2).ShouldBeTrue();
            empty.Equals(zero).ShouldBeFalse();
            empty.Equals(0).ShouldBeFalse();
            empty.Equals(one).ShouldBeFalse();
            empty.Equals(1).ShouldBeFalse();

            zero.Equals(empty).ShouldBeFalse();
            zero.Equals(zero2).ShouldBeTrue();
            zero.Equals(0).ShouldBeTrue();
            zero.Equals(one).ShouldBeFalse();
            zero.Equals(1).ShouldBeFalse();

            one.Equals(empty).ShouldBeFalse();
            one.Equals(zero).ShouldBeFalse();
            one.Equals(0).ShouldBeFalse();
            one.Equals(one2).ShouldBeTrue();
            one.Equals(1).ShouldBeTrue();
        }

    }
}
