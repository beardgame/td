using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Generic;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class Building :
        BuildingBase<Building>,
        IIdable<Building>,
        IDamageSource,
        IListener<ConstructionFinished>,
        IListener<ConstructionStarted>,
        IListener<ReportAdded>,
        IMortal,
        INamed,
        IPlacedBuilding,
        IReportSubject,
        ISyncable
    {

        public Id<Building> Id { get; }
        public string Name { get; }

        private readonly BuildingState mutableState;
        private readonly DamageExecutor damageExecutor;
        private readonly List<IReport> reports = new();
        private ImmutableArray<ISyncable> syncables;

        public IReadOnlyCollection<IReport> Reports { get; }
        private bool isDead;

        public override IBuildingState State { get; }

        public Building(Id<Building> id, IBuildingBlueprint blueprint, Faction faction)
            : base(blueprint, faction)
        {
            Id = id;
            Name = blueprint.Name;

            mutableState = new BuildingState();
            State = mutableState.CreateProxy();

            damageExecutor = new DamageExecutor(Events);

            Reports = reports.AsReadOnly();
        }

        protected override IEnumerable<IComponent<Building>> InitializeComponents()
            => Blueprint.GetComponentsForBuilding();

        public void AttributeDamage(IMortal target, DamageResult damageResult)
        {
            Events.Send(new CausedDamage(target, damageResult));
        }

        public void AttributeKill(IMortal target)
        {
            Events.Send(new CausedKill(target));
        }

        public DamageResult Damage(DamageInfo damage)
        {
            return damageExecutor.Damage(damage);
        }

        public void HandleEvent(ConstructionFinished @event)
        {
            mutableState.IsCompleted = true;
        }

        public void HandleEvent(ConstructionStarted @event)
        {
            materialize();
        }

        public void HandleEvent(ReportAdded @event)
        {
            reports.Add(@event.Report);
        }

        protected override void OnAdded()
        {
            Game.IdAs(this);
            SelectionListener.Create(
                    onFocus: () => mutableState.SelectionState = SelectionState.Focused,
                    onFocusReset: () => mutableState.SelectionState = SelectionState.Default,
                    onSelect: () => mutableState.SelectionState = SelectionState.Selected,
                    onSelectionReset: () => mutableState.SelectionState = SelectionState.Default)
                .Subscribe(Events);
            Events.Subscribe<ConstructionFinished>(this);
            Events.Subscribe<ConstructionStarted>(this);
            Events.Subscribe<ReportAdded>(this);
            base.OnAdded();
        }

        public void OnDeath()
        {
            isDead = true;
        }

        protected override void OnDelete()
        {
            Game.BuildingLayer.RemoveBuilding(this);
            if (State.IsMaterialized)
            {
                OccupiedTileAccumulator.AccumulateOccupiedTiles(this)
                    .ForEach(tile => { Game.Navigator.RemoveSink(tile); });
            }

            base.OnDelete();
        }

        private void materialize()
        {
            mutableState.IsMaterialized = true;

            Game.Meta.Synchronizer.RegisterSyncable(this);
            syncables = Components.Get<ISyncable>().ToImmutableArray();
            OccupiedTileAccumulator.AccumulateOccupiedTiles(this).ForEach(tile => Game.Navigator.AddBackupSink(tile));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if (isDead)
            {
                this.Sync(KillBuilding.Command);
            }
        }

        public override void Draw(CoreDrawers drawers)
        {
            if (!mutableState.IsMaterialized)
            {
                return;
            }

            base.Draw(drawers);
        }

        public IStateToSync GetCurrentStateToSync() =>
            new CompositeStateToSync(syncables.Select(s => s.GetCurrentStateToSync()));
    }
}
