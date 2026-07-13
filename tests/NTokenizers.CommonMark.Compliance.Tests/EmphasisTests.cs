using NTokenizers.ToHtml;

namespace NTokenizers.CommonMark.Compliance.Tests;

/// <summary>
/// CommonMark spec 0.31.2 compliance tests for Emphasis and strong emphasis.
/// Source: https://spec.commonmark.org/0.31.2/#emphasis-and-strong-emphasis
/// Total examples: 132
/// </summary>
public class EmphasisTests
{
    [Fact]
    public void Example_350()
    {
        var input = "a * foo bar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>a * foo bar*</p>", html);
    }

    [Fact]
    public void Example_351()
    {
        var input = "a*\"foo\"*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>a*&quot;foo&quot;*</p>", html);
    }

    [Fact]
    public void Example_352()
    {
        var input = "* a *";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>* a *</p>", html);
    }

    [Fact]
    public void Example_353()
    {
        var input = "*$*alpha.\n\n*£*bravo.\n\n*€*charlie.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*$*alpha.</p>\n<p>*£*bravo.</p>\n<p>*€*charlie.</p>", html);
    }

    [Fact]
    public void Example_354()
    {
        var input = "foo*bar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<em>bar</em></p>", html);
    }

    [Fact]
    public void Example_355()
    {
        var input = "5*6*78";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>5<em>6</em>78</p>", html);
    }

    [Fact]
    public void Example_356()
    {
        var input = "_foo bar_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo bar</em></p>", html);
    }

    [Fact]
    public void Example_357()
    {
        var input = "_ foo bar_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_ foo bar_</p>", html);
    }

    [Fact]
    public void Example_358()
    {
        var input = "a_\"foo\"_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>a_&quot;foo&quot;_</p>", html);
    }

    [Fact]
    public void Example_359()
    {
        var input = "foo_bar_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo_bar_</p>", html);
    }

    [Fact]
    public void Example_360()
    {
        var input = "5_6_78";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>5_6_78</p>", html);
    }

    [Fact]
    public void Example_361()
    {
        var input = "пристаням_стремятся_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>пристаням_стремятся_</p>", html);
    }

    [Fact]
    public void Example_362()
    {
        var input = "aa_\"bb\"_cc";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>aa_&quot;bb&quot;_cc</p>", html);
    }

    [Fact]
    public void Example_363()
    {
        var input = "foo-_(bar)_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo-<em>(bar)</em></p>", html);
    }

    [Fact]
    public void Example_364()
    {
        var input = "_foo*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_foo*</p>", html);
    }

    [Fact]
    public void Example_365()
    {
        var input = "*foo bar *";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*foo bar *</p>", html);
    }

    [Fact]
    public void Example_366()
    {
        var input = "*foo bar\n*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*foo bar\n*</p>", html);
    }

    [Fact]
    public void Example_367()
    {
        var input = "*(*foo)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*(*foo)</p>", html);
    }

    [Fact]
    public void Example_368()
    {
        var input = "*(*foo*)*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>(<em>foo</em>)</em></p>", html);
    }

    [Fact]
    public void Example_369()
    {
        var input = "*foo*bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo</em>bar</p>", html);
    }

    [Fact]
    public void Example_370()
    {
        var input = "_foo bar _";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_foo bar _</p>", html);
    }

    [Fact]
    public void Example_371()
    {
        var input = "_(_foo)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_(_foo)</p>", html);
    }

    [Fact]
    public void Example_372()
    {
        var input = "_(_foo_)_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>(<em>foo</em>)</em></p>", html);
    }

    [Fact]
    public void Example_373()
    {
        var input = "_foo_bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_foo_bar</p>", html);
    }

    [Fact]
    public void Example_374()
    {
        var input = "_пристаням_стремятся";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_пристаням_стремятся</p>", html);
    }

    [Fact]
    public void Example_375()
    {
        var input = "_foo_bar_baz_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo_bar_baz</em></p>", html);
    }

    [Fact]
    public void Example_376()
    {
        var input = "_(bar)_.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>(bar)</em>.</p>", html);
    }

    [Fact]
    public void Example_377()
    {
        var input = "**foo bar**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo bar</strong></p>", html);
    }

    [Fact]
    public void Example_378()
    {
        var input = "** foo bar**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>** foo bar**</p>", html);
    }

    [Fact]
    public void Example_379()
    {
        var input = "a**\"foo\"**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>a**&quot;foo&quot;**</p>", html);
    }

    [Fact]
    public void Example_380()
    {
        var input = "foo**bar**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<strong>bar</strong></p>", html);
    }

    [Fact]
    public void Example_381()
    {
        var input = "__foo bar__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo bar</strong></p>", html);
    }

    [Fact]
    public void Example_382()
    {
        var input = "__ foo bar__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__ foo bar__</p>", html);
    }

    [Fact]
    public void Example_383()
    {
        var input = "__\nfoo bar__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__\nfoo bar__</p>", html);
    }

    [Fact]
    public void Example_384()
    {
        var input = "a__\"foo\"__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>a__&quot;foo&quot;__</p>", html);
    }

    [Fact]
    public void Example_385()
    {
        var input = "foo__bar__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo__bar__</p>", html);
    }

    [Fact]
    public void Example_386()
    {
        var input = "5__6__78";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>5__6__78</p>", html);
    }

    [Fact]
    public void Example_387()
    {
        var input = "пристаням__стремятся__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>пристаням__стремятся__</p>", html);
    }

    [Fact]
    public void Example_388()
    {
        var input = "__foo, __bar__, baz__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo, <strong>bar</strong>, baz</strong></p>", html);
    }

    [Fact]
    public void Example_389()
    {
        var input = "foo-__(bar)__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo-<strong>(bar)</strong></p>", html);
    }

    [Fact]
    public void Example_390()
    {
        var input = "**foo bar **";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>**foo bar **</p>", html);
    }

    [Fact]
    public void Example_391()
    {
        var input = "**(**foo)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>**(**foo)</p>", html);
    }

    [Fact]
    public void Example_392()
    {
        var input = "*(**foo**)*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>(<strong>foo</strong>)</em></p>", html);
    }

    [Fact]
    public void Example_393()
    {
        var input = "**Gomphocarpus (*Gomphocarpus physocarpus*, syn.\n*Asclepias physocarpa*)**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>Gomphocarpus (<em>Gomphocarpus physocarpus</em>, syn.\n<em>Asclepias physocarpa</em>)</strong></p>", html);
    }

    [Fact]
    public void Example_394()
    {
        var input = "**foo \"*bar*\" foo**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo &quot;<em>bar</em>&quot; foo</strong></p>", html);
    }

    [Fact]
    public void Example_395()
    {
        var input = "**foo**bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo</strong>bar</p>", html);
    }

    [Fact]
    public void Example_396()
    {
        var input = "__foo bar __";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__foo bar __</p>", html);
    }

    [Fact]
    public void Example_397()
    {
        var input = "__(__foo)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__(__foo)</p>", html);
    }

    [Fact]
    public void Example_398()
    {
        var input = "_(__foo__)_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>(<strong>foo</strong>)</em></p>", html);
    }

    [Fact]
    public void Example_399()
    {
        var input = "__foo__bar";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__foo__bar</p>", html);
    }

    [Fact]
    public void Example_400()
    {
        var input = "__пристаням__стремятся";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__пристаням__стремятся</p>", html);
    }

    [Fact]
    public void Example_401()
    {
        var input = "__foo__bar__baz__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo__bar__baz</strong></p>", html);
    }

    [Fact]
    public void Example_402()
    {
        var input = "__(bar)__.";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>(bar)</strong>.</p>", html);
    }

    [Fact]
    public void Example_403()
    {
        var input = "*foo [bar](/url)*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <a href=\"/url\">bar</a></em></p>", html);
    }

    [Fact]
    public void Example_404()
    {
        var input = "*foo\nbar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo\nbar</em></p>", html);
    }

    [Fact]
    public void Example_405()
    {
        var input = "_foo __bar__ baz_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <strong>bar</strong> baz</em></p>", html);
    }

    [Fact]
    public void Example_406()
    {
        var input = "_foo _bar_ baz_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <em>bar</em> baz</em></p>", html);
    }

    [Fact]
    public void Example_407()
    {
        var input = "__foo_ bar_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em><em>foo</em> bar</em></p>", html);
    }

    [Fact]
    public void Example_408()
    {
        var input = "*foo *bar**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <em>bar</em></em></p>", html);
    }

    [Fact]
    public void Example_409()
    {
        var input = "*foo **bar** baz*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <strong>bar</strong> baz</em></p>", html);
    }

    [Fact]
    public void Example_410()
    {
        var input = "*foo**bar**baz*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo<strong>bar</strong>baz</em></p>", html);
    }

    [Fact]
    public void Example_411()
    {
        var input = "*foo**bar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo**bar</em></p>", html);
    }

    [Fact]
    public void Example_412()
    {
        var input = "***foo** bar*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em><strong>foo</strong> bar</em></p>", html);
    }

    [Fact]
    public void Example_413()
    {
        var input = "*foo **bar***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <strong>bar</strong></em></p>", html);
    }

    [Fact]
    public void Example_414()
    {
        var input = "*foo**bar***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo<strong>bar</strong></em></p>", html);
    }

    [Fact]
    public void Example_415()
    {
        var input = "foo***bar***baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<em><strong>bar</strong></em>baz</p>", html);
    }

    [Fact]
    public void Example_416()
    {
        var input = "foo******bar*********baz";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo<strong><strong><strong>bar</strong></strong></strong>***baz</p>", html);
    }

    [Fact]
    public void Example_417()
    {
        var input = "*foo **bar *baz* bim** bop*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <strong>bar <em>baz</em> bim</strong> bop</em></p>", html);
    }

    [Fact]
    public void Example_418()
    {
        var input = "*foo [*bar*](/url)*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <a href=\"/url\"><em>bar</em></a></em></p>", html);
    }

    [Fact]
    public void Example_419()
    {
        var input = "** is not an empty emphasis";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>** is not an empty emphasis</p>", html);
    }

    [Fact]
    public void Example_420()
    {
        var input = "**** is not an empty strong emphasis";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>**** is not an empty strong emphasis</p>", html);
    }

    [Fact]
    public void Example_421()
    {
        var input = "**foo [bar](/url)**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo <a href=\"/url\">bar</a></strong></p>", html);
    }

    [Fact]
    public void Example_422()
    {
        var input = "**foo\nbar**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo\nbar</strong></p>", html);
    }

    [Fact]
    public void Example_423()
    {
        var input = "__foo _bar_ baz__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo <em>bar</em> baz</strong></p>", html);
    }

    [Fact]
    public void Example_424()
    {
        var input = "__foo __bar__ baz__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo <strong>bar</strong> baz</strong></p>", html);
    }

    [Fact]
    public void Example_425()
    {
        var input = "____foo__ bar__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong><strong>foo</strong> bar</strong></p>", html);
    }

    [Fact]
    public void Example_426()
    {
        var input = "**foo **bar****";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo <strong>bar</strong></strong></p>", html);
    }

    [Fact]
    public void Example_427()
    {
        var input = "**foo *bar* baz**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo <em>bar</em> baz</strong></p>", html);
    }

    [Fact]
    public void Example_428()
    {
        var input = "**foo*bar*baz**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo<em>bar</em>baz</strong></p>", html);
    }

    [Fact]
    public void Example_429()
    {
        var input = "***foo* bar**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong><em>foo</em> bar</strong></p>", html);
    }

    [Fact]
    public void Example_430()
    {
        var input = "**foo *bar***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo <em>bar</em></strong></p>", html);
    }

    [Fact]
    public void Example_431()
    {
        var input = "**foo *bar **baz**\nbim* bop**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo <em>bar <strong>baz</strong>\nbim</em> bop</strong></p>", html);
    }

    [Fact]
    public void Example_432()
    {
        var input = "**foo [*bar*](/url)**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo <a href=\"/url\"><em>bar</em></a></strong></p>", html);
    }

    [Fact]
    public void Example_433()
    {
        var input = "__ is not an empty emphasis";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__ is not an empty emphasis</p>", html);
    }

    [Fact]
    public void Example_434()
    {
        var input = "____ is not an empty strong emphasis";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>____ is not an empty strong emphasis</p>", html);
    }

    [Fact]
    public void Example_435()
    {
        var input = "foo ***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo ***</p>", html);
    }

    [Fact]
    public void Example_436()
    {
        var input = "foo *\\**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <em>*</em></p>", html);
    }

    [Fact]
    public void Example_437()
    {
        var input = "foo *_*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <em>_</em></p>", html);
    }

    [Fact]
    public void Example_438()
    {
        var input = "foo *****";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo *****</p>", html);
    }

    [Fact]
    public void Example_439()
    {
        var input = "foo **\\***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <strong>*</strong></p>", html);
    }

    [Fact]
    public void Example_440()
    {
        var input = "foo **_**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <strong>_</strong></p>", html);
    }

    [Fact]
    public void Example_441()
    {
        var input = "**foo*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*<em>foo</em></p>", html);
    }

    [Fact]
    public void Example_442()
    {
        var input = "*foo**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo</em>*</p>", html);
    }

    [Fact]
    public void Example_443()
    {
        var input = "***foo**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*<strong>foo</strong></p>", html);
    }

    [Fact]
    public void Example_444()
    {
        var input = "****foo*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>***<em>foo</em></p>", html);
    }

    [Fact]
    public void Example_445()
    {
        var input = "**foo***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo</strong>*</p>", html);
    }

    [Fact]
    public void Example_446()
    {
        var input = "*foo****";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo</em>***</p>", html);
    }

    [Fact]
    public void Example_447()
    {
        var input = "foo ___";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo ___</p>", html);
    }

    [Fact]
    public void Example_448()
    {
        var input = "foo _\\__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <em>_</em></p>", html);
    }

    [Fact]
    public void Example_449()
    {
        var input = "foo _*_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <em>*</em></p>", html);
    }

    [Fact]
    public void Example_450()
    {
        var input = "foo _____";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo _____</p>", html);
    }

    [Fact]
    public void Example_451()
    {
        var input = "foo __\\___";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <strong>_</strong></p>", html);
    }

    [Fact]
    public void Example_452()
    {
        var input = "foo __*__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>foo <strong>*</strong></p>", html);
    }

    [Fact]
    public void Example_453()
    {
        var input = "__foo_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_<em>foo</em></p>", html);
    }

    [Fact]
    public void Example_454()
    {
        var input = "_foo__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo</em>_</p>", html);
    }

    [Fact]
    public void Example_455()
    {
        var input = "___foo__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_<strong>foo</strong></p>", html);
    }

    [Fact]
    public void Example_456()
    {
        var input = "____foo_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>___<em>foo</em></p>", html);
    }

    [Fact]
    public void Example_457()
    {
        var input = "__foo___";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo</strong>_</p>", html);
    }

    [Fact]
    public void Example_458()
    {
        var input = "_foo____";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo</em>___</p>", html);
    }

    [Fact]
    public void Example_459()
    {
        var input = "**foo**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo</strong></p>", html);
    }

    [Fact]
    public void Example_460()
    {
        var input = "*_foo_*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em><em>foo</em></em></p>", html);
    }

    [Fact]
    public void Example_461()
    {
        var input = "__foo__";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong>foo</strong></p>", html);
    }

    [Fact]
    public void Example_462()
    {
        var input = "_*foo*_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em><em>foo</em></em></p>", html);
    }

    [Fact]
    public void Example_463()
    {
        var input = "****foo****";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong><strong>foo</strong></strong></p>", html);
    }

    [Fact]
    public void Example_464()
    {
        var input = "____foo____";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong><strong>foo</strong></strong></p>", html);
    }

    [Fact]
    public void Example_465()
    {
        var input = "******foo******";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><strong><strong><strong>foo</strong></strong></strong></p>", html);
    }

    [Fact]
    public void Example_466()
    {
        var input = "***foo***";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em><strong>foo</strong></em></p>", html);
    }

    [Fact]
    public void Example_467()
    {
        var input = "_____foo_____";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em><strong><strong>foo</strong></strong></em></p>", html);
    }

    [Fact]
    public void Example_468()
    {
        var input = "*foo _bar* baz_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo _bar</em> baz_</p>", html);
    }

    [Fact]
    public void Example_469()
    {
        var input = "*foo __bar *baz bim__ bam*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>foo <strong>bar *baz bim</strong> bam</em></p>", html);
    }

    [Fact]
    public void Example_470()
    {
        var input = "**foo **bar baz**";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>**foo <strong>bar baz</strong></p>", html);
    }

    [Fact]
    public void Example_471()
    {
        var input = "*foo *bar baz*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*foo <em>bar baz</em></p>", html);
    }

    [Fact]
    public void Example_472()
    {
        var input = "*[bar*](/url)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*<a href=\"/url\">bar*</a></p>", html);
    }

    [Fact]
    public void Example_473()
    {
        var input = "_foo [bar_](/url)";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>_foo <a href=\"/url\">bar_</a></p>", html);
    }

    [Fact]
    public void Example_474()
    {
        var input = "*<img src=\"foo\" title=\"*\"/>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>*<img src=\"foo\" title=\"*\"/></p>", html);
    }

    [Fact]
    public void Example_475()
    {
        var input = "**<a href=\"**\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>**<a href=\"**\"></p>", html);
    }

    [Fact]
    public void Example_476()
    {
        var input = "__<a href=\"__\">";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__<a href=\"__\"></p>", html);
    }

    [Fact]
    public void Example_477()
    {
        var input = "*a `*`*";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>a <code>*</code></em></p>", html);
    }

    [Fact]
    public void Example_478()
    {
        var input = "_a `_`_";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><em>a <code>_</code></em></p>", html);
    }

    [Fact]
    public void Example_479()
    {
        var input = "**a<https://foo.bar/?q=**>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>**a<a href=\"https://foo.bar/?q=**\">https://foo.bar/?q=**</a></p>", html);
    }

    [Fact]
    public void Example_480()
    {
        var input = "__a<https://foo.bar/?q=__>";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p>__a<a href=\"https://foo.bar/?q=__\">https://foo.bar/?q=__</a></p>", html);
    }

    [Fact]
    public void Example_481()
    {
        var input = "[link](/uri \"title\")";
        var html = MarkdownConverter.ToHtml(input);
        Assert.Equal("<p><a href=\"/uri\" title=\"title\">link</a></p>", html);
    }

}
