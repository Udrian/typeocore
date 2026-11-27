using Avalonia.Controls;
using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeDCore.ViewModel.Dialogs.Project
{
    internal class CreateEntityTypeViewModel : CreateComponentTypeBaseViewModel
    {
        // Properties
        public bool ComponentUpdatable { get; set; } = true;
        public bool ComponentDrawable { get; set; } = true;
        public bool? ComponentUpdatableSet { get; set; }
        public bool? ComponentDrawableSet { get; set; }
        public bool ComponentUpdatableEnabled { get; set; }
        public bool ComponentDrawableEnabled { get; set; }
        public string UpdatableDescription => ComponentUpdatableEnabled
            ? "Calls Update(dt) while active." : $"Inherited from {ParentComponent.ClassName}.";
        public string DrawableDescription => ComponentDrawableEnabled
            ? "Provides Draw(canvas); implement it to draw this entity." : $"Inherited from {ParentComponent.ClassName}.";

        // Constructors
        public CreateEntityTypeViewModel(Control element, TypeD.Models.Data.Project project, string @namespace, Component componentBaseType) : base(element, project, @namespace, componentBaseType)
        {
            ComponentUpdatableEnabled = true;
            ComponentDrawableEnabled = true;
        }

        // Functions
        public override void OnParentComponentSet(Component parentComponent)
        {
            base.OnParentComponentSet(parentComponent);

            if (parentComponent.IsBasedOn(typeof(Entity)) && parentComponent.HasInterface(typeof(IUpdatable)))
            {
                if (!ComponentUpdatableSet.HasValue)
                    ComponentUpdatableSet = ComponentUpdatable;

                ComponentUpdatable = true;
                ComponentUpdatableEnabled = false;
                OnPropertyChanged(nameof(ComponentUpdatable));
                OnPropertyChanged(nameof(ComponentUpdatableEnabled));
            }
            else if(ComponentUpdatableSet.HasValue)
            {
                ComponentUpdatable = ComponentUpdatableSet.Value;
                ComponentUpdatableEnabled = true;
                OnPropertyChanged(nameof(ComponentUpdatable));
                OnPropertyChanged(nameof(ComponentUpdatableEnabled));

                ComponentUpdatableSet = null;
            }

            if (parentComponent.IsBasedOn(typeof(Entity)) && parentComponent.HasInterface(typeof(IDrawable)))
            {
                if (!ComponentDrawableSet.HasValue)
                    ComponentDrawableSet = ComponentDrawable;

                ComponentDrawable = true;
                ComponentDrawableEnabled = false;
                OnPropertyChanged(nameof(ComponentDrawable));
                OnPropertyChanged(nameof(ComponentDrawableEnabled));
            }
            else if(ComponentDrawableSet.HasValue)
            {
                ComponentDrawable = ComponentDrawableSet.Value;
                ComponentDrawableEnabled = true;
                OnPropertyChanged(nameof(ComponentDrawable));
                OnPropertyChanged(nameof(ComponentDrawableEnabled));

                ComponentDrawableSet = null;
            }
            OnPropertyChanged(nameof(UpdatableDescription));
            OnPropertyChanged(nameof(DrawableDescription));
        }
    }
}
