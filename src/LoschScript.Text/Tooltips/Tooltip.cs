using System.Collections.ObjectModel;
using System.Linq;

namespace LoschScript.Text.Tooltips;

/// <summary>
/// Represents a tooltip as a sequence of <see cref="Word"/>s.
/// </summary>
public class Tooltip
{
    private ObservableCollection<Word> _words = new();
    /// <summary>
    /// The words representing the current tooltip.
    /// </summary>
    public ObservableCollection<Word> Words
    {
        get => _words;

        set
        {
            _words = value;
            RawText = new(_words.Select(w => w.Text).SelectMany(s => s.ToCharArray()).ToArray());
        }
    }

    /// <summary>
    /// The raw text of the current tooltip without styling information.
    /// </summary>
    public string RawText { get; private set; }
}