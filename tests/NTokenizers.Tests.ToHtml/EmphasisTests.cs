using NTokenizers.ToHtml;

namespace NTokenizers.Tests.ToHtml;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Emphasis and strong emphasis.
/// Source: https://spec.commonmark.org/0.31.2/#emphasis-and-strong-emphasis
/// Total examples: 132
/// </summary>
public class EmphasisTests
{
    [Fact]
    public void Example_349()
    {
        var input = "*foo bar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo bar</em></p>", html);
    }

    [Fact]
    public void Example_350()
    {
        var input = "a * foo bar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>a <em> foo bar</em></p>", html);
    }

    [Fact]
    public void Example_351()
    {
        var input = "a*\"foo\"*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>a<em>&quot;foo&quot;</em></p>", html);
    }

    [Fact]
    public void Example_352()
    {
        var input = "* a *";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em> a </em></p>", html);
    }

    [Fact]
    public void Example_353()
    {
        var input = "*$*alpha.\n\n*£*bravo.\n\n*€*charlie.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>$</em>alpha.</p>\n<p><em>£</em>bravo.</p>\n<p><em>€</em>charlie.</p>", html);
    }

    [Fact]
    public void Example_354()
    {
        var input = "foo*bar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo<em>bar</em></p>", html);
    }

    [Fact]
    public void Example_355()
    {
        var input = "5*6*78";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>5<em>6</em>78</p>", html);
    }

    [Fact]
    public void Example_356()
    {
        var input = "_foo bar_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo bar</em></p>", html);
    }

    [Fact]
    public void Example_357()
    {
        var input = "_foo bar_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo bar</em></p>", html);
    }

    [Fact]
    public void Example_358()
    {
        var input = "a_\"foo\"_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>a<em>&quot;foo&quot;</em></p>", html);
    }

    [Fact]
    public void Example_359()
    {
        var input = "foo_bar_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo<em>bar</em></p>", html);
    }

    [Fact]
    public void Example_360()
    {
        var input = "5_6_78";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>5<em>6</em>78</p>", html);
    }

    [Fact]
    public void Example_361()
    {
        var input = "пристаням_стремятся_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>пристаням<em>стремятся</em></p>", html);
    }

    [Fact]
    public void Example_362()
    {
        var input = "aa_\"bb\"_cc";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>aa<em>&quot;bb&quot;</em>cc</p>", html);
    }

    [Fact]
    public void Example_363()
    {
        var input = "foo-_(bar)_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo-<em>(bar)</em></p>", html);
    }

    [Fact]
    public void Example_364()
    {
        var input = "_foo*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>_foo*</p>", html);
    }

    [Fact]
    public void Example_365()
    {
        var input = "*foo bar *";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo bar </em></p>", html);
    }

    [Fact]
    public void Example_366()
    {
        var input = "*foo bar\n*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo bar<br/></em></p>", html);
    }

    [Fact]
    public void Example_367()
    {
        var input = "*(*foo)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>(</em>foo)</p>", html);
    }

    [Fact]
    public void Example_368()
    {
        var input = "*(*foo*)*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>(</em>foo<em>)</em></p>", html);
    }

    [Fact]
    public void Example_369()
    {
        var input = "*foo*bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em>bar</p>", html);
    }

    [Fact]
    public void Example_370()
    {
        var input = "_foo bar _";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo bar </em></p>", html);
    }

    [Fact]
    public void Example_371()
    {
        var input = "_(_foo)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>(</em>foo)</p>", html);
    }

    [Fact]
    public void Example_372()
    {
        var input = "_(_foo_)_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>(</em>foo<em>)</em></p>", html);
    }

    [Fact]
    public void Example_373()
    {
        var input = "_foo_bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em>bar</p>", html);
    }

    [Fact]
    public void Example_374()
    {
        var input = "_пристаням_стремятся";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>пристаням</em>стремятся</p>", html);
    }

    [Fact]
    public void Example_375()
    {
        var input = "_foo_bar_baz_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em>bar<em>baz</em></p>", html);
    }

    [Fact]
    public void Example_376()
    {
        var input = "_(bar)_.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>(bar)</em>.</p>", html);
    }

    [Fact]
    public void Example_377()
    {
        var input = "**foo bar**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo bar</strong></p>", html);
    }

    [Fact]
    public void Example_378()
    {
        var input = "** foo bar**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong> foo bar</strong></p>", html);
    }

    [Fact]
    public void Example_379()
    {
        var input = "a**\"foo\"**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>a<strong>&quot;foo&quot;</strong></p>", html);
    }

    [Fact]
    public void Example_380()
    {
        var input = "foo**bar**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo<strong>bar</strong></p>", html);
    }

    [Fact]
    public void Example_381()
    {
        var input = "__foo bar__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo bar</strong></p>", html);
    }

    [Fact]
    public void Example_382()
    {
        var input = "__ foo bar__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong> foo bar</strong></p>", html);
    }

    [Fact]
    public void Example_383()
    {
        var input = "__\nfoo bar__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong><br/>foo bar</strong></p>", html);
    }

    [Fact]
    public void Example_384()
    {
        var input = "a__\"foo\"__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>a<strong>&quot;foo&quot;</strong></p>", html);
    }

    [Fact]
    public void Example_385()
    {
        var input = "foo__bar__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo<strong>bar</strong></p>", html);
    }

    [Fact]
    public void Example_386()
    {
        var input = "5__6__78";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>5<strong>6</strong>78</p>", html);
    }

    [Fact]
    public void Example_387()
    {
        var input = "пристаням__стремятся__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>пристаням<strong>стремятся</strong></p>", html);
    }

    [Fact]
    public void Example_388()
    {
        var input = "__foo, __bar__, baz__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo, </strong>bar<strong>, baz</strong></p>", html);
    }

    [Fact]
    public void Example_389()
    {
        var input = "foo-__(bar)__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo-<strong>(bar)</strong></p>", html);
    }

    [Fact]
    public void Example_390()
    {
        var input = "**foo bar **";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo bar </strong></p>", html);
    }

    [Fact]
    public void Example_391()
    {
        var input = "**(**foo)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>(</strong>foo)</p>", html);
    }

    [Fact]
    public void Example_392()
    {
        var input = "*(**foo**)*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>(</em><em>foo</em><em>)</em></p>", html);
    }

    [Fact]
    public void Example_393()
    {
        var input = "**Gomphocarpus (*Gomphocarpus physocarpus*, syn.\n*Asclepias physocarpa*)**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>Gomphocarpus (*Gomphocarpus physocarpus*, syn.<br/>*Asclepias physocarpa*)</strong></p>", html);
    }

    [Fact]
    public void Example_394()
    {
        var input = "**foo \"*bar*\" foo**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo &quot;*bar*&quot; foo</strong></p>", html);
    }

    [Fact]
    public void Example_395()
    {
        var input = "**foo**bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo</strong>bar</p>", html);
    }

    [Fact]
    public void Example_396()
    {
        var input = "__foo bar __";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo bar </strong></p>", html);
    }

    [Fact]
    public void Example_397()
    {
        var input = "__(__foo)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>(</strong>foo)</p>", html);
    }

    [Fact]
    public void Example_398()
    {
        var input = "_(__foo__)_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>(</em><em>foo</em><em>)</em></p>", html);
    }

    [Fact]
    public void Example_399()
    {
        var input = "__foo__bar";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo</strong>bar</p>", html);
    }

    [Fact]
    public void Example_400()
    {
        var input = "__пристаням__стремятся";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>пристаням</strong>стремятся</p>", html);
    }

    [Fact]
    public void Example_401()
    {
        var input = "__foo__bar__baz__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo</strong>bar<strong>baz</strong></p>", html);
    }

    [Fact]
    public void Example_402()
    {
        var input = "__(bar)__.";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>(bar)</strong>.</p>", html);
    }

    [Fact]
    public void Example_403()
    {
        var input = "*foo [bar](/url)*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo [bar](/url)</em></p>", html);
    }

    [Fact]
    public void Example_404()
    {
        var input = "*foo\nbar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo<br/>bar</em></p>", html);
    }

    [Fact]
    public void Example_405()
    {
        var input = "_foo __bar__ baz_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo </em><em>bar</em><em> baz</em></p>", html);
    }

    [Fact]
    public void Example_406()
    {
        var input = "_foo _bar_ baz_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo </em>bar<em> baz</em></p>", html);
    }

    [Fact]
    public void Example_407()
    {
        var input = "__foo_ bar_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>__foo_ bar_</p>", html);
    }

    [Fact]
    public void Example_408()
    {
        var input = "*foo *bar**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo </em>bar**</p>", html);
    }

    [Fact]
    public void Example_409()
    {
        var input = "*foo **bar** baz*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo </em><em>bar</em><em> baz</em></p>", html);
    }

    [Fact]
    public void Example_410()
    {
        var input = "*foo**bar**baz*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em><em>bar</em><em>baz</em></p>", html);
    }

    [Fact]
    public void Example_411()
    {
        var input = "*foo**bar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em><em>bar</em></p>", html);
    }

    [Fact]
    public void Example_412()
    {
        var input = "***foo** bar*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>*foo</strong> bar*</p>", html);
    }

    [Fact]
    public void Example_413()
    {
        var input = "*foo **bar***";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo </em><em>bar</em>**</p>", html);
    }

    [Fact]
    public void Example_414()
    {
        var input = "*foo**bar***";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em><em>bar</em>**</p>", html);
    }

    [Fact]
    public void Example_415()
    {
        var input = "foo***bar***baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo<strong>*bar</strong>*baz</p>", html);
    }

    [Fact]
    public void Example_416()
    {
        var input = "foo******bar*********baz";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo<strong></strong><strong>bar</strong><strong></strong>***baz</p>", html);
    }

    [Fact]
    public void Example_417()
    {
        var input = "*foo **bar *baz* bim** bop*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo </em><em>bar </em>baz<em> bim</em><em> bop</em></p>", html);
    }

    [Fact]
    public void Example_418()
    {
        var input = "*foo [*bar*](/url)*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo [</em>bar<em>](/url)</em></p>", html);
    }

    [Fact]
    public void Example_419()
    {
        var input = "** is not an empty emphasis";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>** is not an empty emphasis</p>", html);
    }

    [Fact]
    public void Example_420()
    {
        var input = "**** is not an empty strong emphasis";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong> is not an empty strong emphasis</p>", html);
    }

    [Fact]
    public void Example_421()
    {
        var input = "**foo [bar](/url)**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo [bar](/url)</strong></p>", html);
    }

    [Fact]
    public void Example_422()
    {
        var input = "**foo\nbar**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo<br/>bar</strong></p>", html);
    }

    [Fact]
    public void Example_423()
    {
        var input = "__foo _bar_ baz__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo _bar_ baz</strong></p>", html);
    }

    [Fact]
    public void Example_424()
    {
        var input = "__foo __bar__ baz__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo </strong>bar<strong> baz</strong></p>", html);
    }

    [Fact]
    public void Example_425()
    {
        var input = "____foo__ bar__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong>foo<strong> bar</strong></p>", html);
    }

    [Fact]
    public void Example_426()
    {
        var input = "**foo **bar****";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo </strong>bar<strong></strong></p>", html);
    }

    [Fact]
    public void Example_427()
    {
        var input = "**foo *bar* baz**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo *bar* baz</strong></p>", html);
    }

    [Fact]
    public void Example_428()
    {
        var input = "**foo*bar*baz**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo*bar*baz</strong></p>", html);
    }

    [Fact]
    public void Example_429()
    {
        var input = "***foo* bar**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>*foo* bar</strong></p>", html);
    }

    [Fact]
    public void Example_430()
    {
        var input = "**foo *bar***";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo *bar</strong>*</p>", html);
    }

    [Fact]
    public void Example_431()
    {
        var input = "**foo *bar **baz**\nbim* bop**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo *bar </strong>baz<strong><br/>bim* bop</strong></p>", html);
    }

    [Fact]
    public void Example_432()
    {
        var input = "**foo [*bar*](/url)**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo [*bar*](/url)</strong></p>", html);
    }

    [Fact]
    public void Example_433()
    {
        var input = "__ is not an empty emphasis";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>__ is not an empty emphasis</p>", html);
    }

    [Fact]
    public void Example_434()
    {
        var input = "____ is not an empty strong emphasis";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong> is not an empty strong emphasis</p>", html);
    }

    [Fact]
    public void Example_435()
    {
        var input = "foo ***";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo ***</p>", html);
    }

    [Fact]
    public void Example_436()
    {
        var input = "foo *\\**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <em>*</em></p>", html);
    }

    [Fact]
    public void Example_437()
    {
        var input = "foo *_*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <em>_</em></p>", html);
    }

    [Fact]
    public void Example_438()
    {
        var input = "foo *****";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <strong></strong>*</p>", html);
    }

    [Fact]
    public void Example_439()
    {
        var input = "foo **\\***";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <strong>*</strong></p>", html);
    }

    [Fact]
    public void Example_440()
    {
        var input = "foo **_**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <strong>_</strong></p>", html);
    }

    [Fact]
    public void Example_441()
    {
        var input = "**foo*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>**foo*</p>", html);
    }

    [Fact]
    public void Example_442()
    {
        var input = "*foo**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em>*</p>", html);
    }

    [Fact]
    public void Example_443()
    {
        var input = "***foo**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>*foo</strong></p>", html);
    }

    [Fact]
    public void Example_444()
    {
        var input = "****foo*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong>foo*</p>", html);
    }

    [Fact]
    public void Example_445()
    {
        var input = "**foo***";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo</strong>*</p>", html);
    }

    [Fact]
    public void Example_446()
    {
        var input = "*foo****";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em>***</p>", html);
    }

    [Fact]
    public void Example_447()
    {
        var input = "foo ___";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo ___</p>", html);
    }

    [Fact]
    public void Example_448()
    {
        var input = "foo _\\__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <em>_</em></p>", html);
    }

    [Fact]
    public void Example_449()
    {
        var input = "foo _*_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <em>*</em></p>", html);
    }

    [Fact]
    public void Example_450()
    {
        var input = "foo _____";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <strong></strong>_</p>", html);
    }

    [Fact]
    public void Example_451()
    {
        var input = "foo __\\___";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <strong>_</strong></p>", html);
    }

    [Fact]
    public void Example_452()
    {
        var input = "foo __*__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>foo <strong>*</strong></p>", html);
    }

    [Fact]
    public void Example_453()
    {
        var input = "__foo_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p>__foo_</p>", html);
    }

    [Fact]
    public void Example_454()
    {
        var input = "_foo__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em>_</p>", html);
    }

    [Fact]
    public void Example_455()
    {
        var input = "___foo__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>_foo</strong></p>", html);
    }

    [Fact]
    public void Example_456()
    {
        var input = "____foo_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong>foo_</p>", html);
    }

    [Fact]
    public void Example_457()
    {
        var input = "__foo___";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo</strong>_</p>", html);
    }

    [Fact]
    public void Example_458()
    {
        var input = "_foo____";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo</em>___</p>", html);
    }

    [Fact]
    public void Example_459()
    {
        var input = "**foo**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo</strong></p>", html);
    }

    [Fact]
    public void Example_460()
    {
        var input = "*_foo_*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>_foo_</em></p>", html);
    }

    [Fact]
    public void Example_461()
    {
        var input = "__foo__";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo</strong></p>", html);
    }

    [Fact]
    public void Example_462()
    {
        var input = "_*foo*_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>*foo*</em></p>", html);
    }

    [Fact]
    public void Example_463()
    {
        var input = "****foo****";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong>foo<strong></strong></p>", html);
    }

    [Fact]
    public void Example_464()
    {
        var input = "____foo____";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong>foo<strong></strong></p>", html);
    }

    [Fact]
    public void Example_465()
    {
        var input = "******foo******";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong><strong>foo</strong><strong></strong></p>", html);
    }

    [Fact]
    public void Example_466()
    {
        var input = "***foo***";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>*foo</strong>*</p>", html);
    }

    [Fact]
    public void Example_467()
    {
        var input = "_____foo_____";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong></strong><em>foo</em><strong></strong></p>", html);
    }

    [Fact]
    public void Example_468()
    {
        var input = "*foo _bar* baz_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo _bar</em> baz_</p>", html);
    }

    [Fact]
    public void Example_469()
    {
        var input = "*foo __bar *baz bim__ bam*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo __bar </em>baz bim__ bam*</p>", html);
    }

    [Fact]
    public void Example_470()
    {
        var input = "**foo **bar baz**";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>foo </strong>bar baz**</p>", html);
    }

    [Fact]
    public void Example_471()
    {
        var input = "*foo *bar baz*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo </em>bar baz*</p>", html);
    }

    [Fact]
    public void Example_472()
    {
        var input = "*[bar*](/url)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>[bar</em>](/url)</p>", html);
    }

    [Fact]
    public void Example_473()
    {
        var input = "_foo [bar_](/url)";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>foo [bar</em>](/url)</p>", html);
    }

    [Fact]
    public void Example_474()
    {
        var input = "*<img src=\"foo\" title=\"*\"/>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>&lt;img src=&quot;foo&quot; title=&quot;</em>&quot;/&gt;</p>", html);
    }

    [Fact]
    public void Example_475()
    {
        var input = "**<a href=\"**\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>&lt;a href=&quot;</strong>&quot;&gt;</p>", html);
    }

    [Fact]
    public void Example_476()
    {
        var input = "__<a href=\"__\">";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>&lt;a href=&quot;</strong>&quot;&gt;</p>", html);
    }

    [Fact]
    public void Example_477()
    {
        var input = "*a `*`*";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>a `</em>`*</p>", html);
    }

    [Fact]
    public void Example_478()
    {
        var input = "_a `_`_";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><em>a `</em>`_</p>", html);
    }

    [Fact]
    public void Example_479()
    {
        var input = "**a<https://foo.bar/?q=**>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>a&lt;https://foo.bar/?q=</strong>&gt;</p>", html);
    }

    [Fact]
    public void Example_480()
    {
        var input = "__a<https://foo.bar/?q=__>";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><strong>a&lt;https://foo.bar/?q=</strong>&gt;</p>", html);
    }

    [Fact]
    public void Example_481()
    {
        var input = "[link](/uri \"title\")";
        var html = MarkdownConverter.ToHtml(input).Replace("\r\n", "\n");
        Assert.Equal("<p><a href=\"/uri\" title=\"title\">link</a></p>", html);
    }

}