using Assets.Contracts.Economy;
using Assets.Contracts.Infrastructure;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Contracts
{
    public abstract class TileBase : MonoBehaviour
    {
        public TileTerrainType TileTerrainType;

        public bool IsSelected = false;

        public Color DefaultSelectionColor = Color.yellow;

        public Color SelectionColor = Color.yellow;

        public Color HoverColor = Color.red;

        public List<IResource> Resources;

        public List<IBuilding> Buildings;

        public ICountry Owner;

        public IProvince Province;

        public Position Position = new Position();

        public abstract void SetColor(Color color);

        public abstract IEnumerable<Vector3> GetVertices(Direction direction, bool relative = false);

        public abstract void Select(Color color);

        public abstract void Deselect();
    }
}