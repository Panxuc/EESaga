namespace EESaga.Scripts.UI;

using Godot;
using Interfaces;
using System;
using System.Collections.Generic;

public partial class CardBattle : Control
{
    private Control _hand;
    private Control _deck;
    private Control _discard;

    private CardDetail _cardDetail;

    private TextureButton _deckButton;
    private Label _deckCardNum;
    private TextureButton _discardButton;
    private Label _discardCardNum;

    private CardViewer _deckCardViewer;
    private CardViewer _discardCardViewer;

    private BattleCards _battleCards;
    public BattleCards BattleCards
    {
        get => _battleCards;
        set
        {
            _battleCards = value;
            UpdateHandCard();
        }
    }

    private Card _selectedCard;
    public Card SelectedCard
    {
        get => _selectedCard;
        set
        {
            _selectedCard = value;
            _cardDetail.Update(_selectedCard);
        }
    }
    private Card _operatingCard;

    private static readonly PackedScene _cardScene = GD.Load<PackedScene>("res://Scenes/UI/card.tscn");

    private const int _maxHandSize = 8;
    private const float _cardX = 64.0f;
    private const float _cardY = 72.0f;
    private const float _cardRotation = 0.1f;
    private const float _cardWidth = 57.0f;
    private const float _cardHeight = 88.0f;

    public override void _Ready()
    {
        _hand = GetNode<Control>("Hand");
        _deck = GetNode<Control>("Deck");
        _discard = GetNode<Control>("Discard");

        _cardDetail = GetNode<CardDetail>("CardDetail");

        _deckButton = GetNode<TextureButton>("Deck/TextureButton");
        _deckCardNum = GetNode<Label>("Deck/Label");
        _discardButton = GetNode<TextureButton>("Discard/TextureButton");
        _discardCardNum = GetNode<Label>("Discard/Label");

        _deckCardViewer = GetNode<CardViewer>("DeckCardViewer");
        _discardCardViewer = GetNode<CardViewer>("DiscardCardViewer");

        _selectedCard = null;
        _operatingCard = null;

        _deckButton.Pressed += () =>
        {
            _deckCardViewer.DisplayCards(BattleCards.DeckCards);
        };

        _discardButton.Pressed += () =>
        {
            _discardCardViewer.DisplayCards(BattleCards.DiscardCards);
        };
    }

    private void UpdateHandCard()
    {
        var oldCards = _hand.GetChildren();
        foreach (var card in oldCards)
        {
            card.QueueFree();
        }
        foreach (var card in BattleCards.HandCards)
        {
            AddCard(card);
        }
        _deckCardNum.Text = BattleCards.DeckCards.Count.ToString();
        _discardCardNum.Text = BattleCards.DiscardCards.Count.ToString();
    }

    private void UpdateCardPosition()
    {
        var cards = _hand.GetChildren();
        if (cards.Count <= _maxHandSize)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var r = (cards.Count * _cardX) / Math.Tan(cards.Count * _cardRotation / 2) / double.Pi;
                var card = cards[i] as Card;
                var targetRotation = (i + 0.5f) * _cardRotation - cards.Count * _cardRotation / 2;
                var targetPosition = new Vector2(
                    (float)((i + 1) * _cardX - cards.Count * _cardX / 2 - _cardWidth * Math.Cos(targetRotation)),
                    (float)(-_cardY - _cardWidth / 2 * Math.Sin(targetRotation) + r * (1 - Math.Cos(targetRotation)))
                );
                var tweenPosition = CreateTween();
                var tweenRotation = CreateTween();
                var tweenScale = CreateTween();
                tweenPosition.TweenProperty(card, "position", targetPosition, 0.15);
                tweenRotation.TweenProperty(card, "rotation", targetRotation, 0.15);
                tweenScale.TweenProperty(card, "scale", Vector2.One, 0.2);
            }
        }
        else
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var r = (_maxHandSize * _cardX) / Math.Tan(_maxHandSize * _cardRotation / 2) / double.Pi;
                var card = cards[i] as Card;
                var targetRotation = (i + 0.5f) * _maxHandSize * _cardRotation / cards.Count - _maxHandSize * _cardRotation / 2;
                var targetPosition = new Vector2(
                     (float)((i + 0.5f) * _maxHandSize * _cardX / cards.Count - _maxHandSize * _cardX / 2 - _cardWidth / 2 * Math.Cos(targetRotation)),
                     (float)(-_cardY - _cardWidth / 2 * Math.Sin(targetRotation) + r * (1 - Math.Cos(targetRotation)))
                );
                var tweenPosition = CreateTween();
                var tweenRotation = CreateTween();
                var tweenScale = CreateTween();
                tweenPosition.TweenProperty(card, "position", targetPosition, 0.15);
                tweenRotation.TweenProperty(card, "rotation", targetRotation, 0.15);
                tweenScale.TweenProperty(card, "scale", Vector2.One, 0.2f);
            }
        }
    }

    private void AddCard(CardType cardType, string cardName, string cardDescription, int cardCost, CardTarget cardTarget)
    {
        var card = _cardScene.Instantiate<Card>();
        card.InitializeCard(cardType, cardName, cardDescription, cardCost, cardTarget);
        card.Position = _deck.Position - _hand.Position;
        card.Scale = Vector2.Zero;
        card.MouseEntered += () =>
        {
            if (_operatingCard != null) return;
            SelectedCard = card;
            PreviewCard(card);
        };
        _hand.AddChild(card);
        UpdateCardPosition();
    }

    private void AddCard(CardInfo card)
    {
        var cardNode = _cardScene.Instantiate<Card>();
        cardNode.InitializeCard(card);
        cardNode.Position = _deck.Position - _hand.Position;
        cardNode.Scale = Vector2.Zero;
        cardNode.MouseEntered += () =>
        {
            if (_operatingCard != null) return;
            SelectedCard = cardNode;
            PreviewCard(cardNode);
        };
        _hand.AddChild(cardNode);
        UpdateCardPosition();
    }

    private void RemoveCard(Card card)
    {
        if (!_hand.GetChildren().Contains(card)) return;
        var newCard = _cardScene.Instantiate<Card>();
        newCard.Name = "RemovedCard";
        newCard.InitializeCard(card.CardType, card.CardName, card.CardDescription, card.CardCost, card.CardTarget);
        newCard.Position = card.Position + _hand.Position;
        newCard.Rotation = card.Rotation;
        newCard.Scale = card.Scale;
        AddChild(newCard);
        _hand.RemoveChild(card);
        var tweenPosition = CreateTween();
        var tweenRotation = CreateTween();
        var tweenScale = CreateTween();
        tweenPosition.TweenProperty(newCard, "position", _discard.Position, 0.15);
        tweenRotation.TweenProperty(newCard, "rotation", 0.0f, 0.15);
        tweenScale.TweenProperty(newCard, "scale", Vector2.Zero, 0.2);
        tweenScale.TweenCallback(Callable.From(newCard.QueueFree));
        UpdateCardPosition();
    }

    private void RemoveCard(int index)
    {
        if (index < 0 || index >= _hand.GetChildCount()) return;
        var card = _hand.GetChild(index) as Card;
        var newCard = _cardScene.Instantiate<Card>();
        newCard.Name = "RemovedCard";
        newCard.InitializeCard(card.CardType, card.CardName, card.CardDescription, card.CardCost, card.CardTarget);
        newCard.Position = card.Position + _hand.Position;
        newCard.Rotation = card.Rotation;
        newCard.Scale = card.Scale;
        AddChild(newCard);
        _hand.RemoveChild(card);
        var tweenPosition = CreateTween();
        var tweenRotation = CreateTween();
        var tweenScale = CreateTween();
        tweenPosition.TweenProperty(newCard, "position", _discard.Position, 0.15);
        tweenRotation.TweenProperty(newCard, "rotation", 0.0f, 0.15);
        tweenScale.TweenProperty(newCard, "scale", Vector2.Zero, 0.2);
        tweenScale.TweenCallback(Callable.From(newCard.QueueFree));
        UpdateCardPosition();
    }

    private void PreviewCard(Card card)
    {
        if (card == null) return;
        var previewCard = GetNodeOrNull("PreviewCard") as Card;
        if (previewCard != null)
        {
            previewCard.Parent.Visible = true;
            previewCard.Free();
        }
        var newCard = _cardScene.Instantiate<Card>();
        newCard.Name = "PreviewCard";
        newCard.InitializeCard(card.CardType, card.CardName, card.CardDescription, card.CardCost, card.CardTarget);
        newCard.Position = card.Position + _hand.Position;
        newCard.Rotation = card.Rotation;
        newCard.Scale = card.Scale;
        newCard.Parent = card;
        newCard.GuiInput += OperateCard;
        newCard.MouseExited += ExitPreviewCard;
        AddChild(newCard);
        var tweenPosition = CreateTween();
        var tweenRotation = CreateTween();
        var tweenScale = CreateTween();
        tweenPosition.TweenProperty(newCard, "position",
            new Vector2(card.Position.X - _cardWidth / 2, -_cardHeight * 1.4f) + _hand.Position, 0.05);
        tweenRotation.TweenProperty(newCard, "rotation", 0.0f, 0.05);
        tweenScale.TweenProperty(newCard, "scale", Vector2.One * 2, 0.05);
        card.Visible = false;
    }

    private void ExitPreviewCard()
    {
        var newCard = GetNodeOrNull("PreviewCard") as Card;
        SelectedCard = null;
        var tweenPosition = CreateTween();
        var tweenRotation = CreateTween();
        var tweenScale = CreateTween();
        tweenPosition.TweenProperty(newCard, "position", newCard.Parent.Position + _hand.Position, 0.05);
        tweenRotation.TweenProperty(newCard, "rotation", newCard.Parent.Rotation, 0.05);
        tweenScale.TweenProperty(newCard, "scale", newCard.Parent.Scale, 0.05);
        tweenScale.TweenCallback(Callable.From(newCard.Free));
        newCard.Parent.Visible = true;
    }

    private void OperateCard(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.IsReleased())
                {
                    var previewCard = GetNodeOrNull("PreviewCard") as Card;
                    if (previewCard != null)
                    {
                        if (_operatingCard != previewCard)
                        {
                            _operatingCard = previewCard;
                            previewCard.MouseExited -= ExitPreviewCard;
                        }
                        else
                        {
                            _operatingCard = null;
                            previewCard.MouseExited += ExitPreviewCard;
                        }
                    }
                }
            }
        }
    }
}

public struct BattleCards
{
    public List<CardInfo> DeckCards { get; set; }
    public List<CardInfo> HandCards { get; set; }
    public List<CardInfo> DiscardCards { get; set; }
}
