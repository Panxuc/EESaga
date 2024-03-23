namespace EESaga.Scripts.Cards.CardSpecials;

using Entities;
using Godot;
using System.Collections.Generic;
public partial class CardCure : CardSpecial
{
	public static new CardCure Instance() => GD.Load<PackedScene>("res://Scenes/Cards/CardSpecials/card_cure.tscn").Instantiate<CardCure>();

	public override void TakeEffect(List<BattlePiece> battlePieces)
	{
		foreach (var piece in battlePieces)
		{
			var _health = piece.Health;
			piece.Health += (piece.HealthMax - piece.Health) / 3;
			GD.Print($"{piece.Name} 回复了 {piece.Health - _health} 点生命");
		}
	}
}