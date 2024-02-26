using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories;

static class EntityStatusFactories
{
    [Obsolete("Use in-game overlays")]
    public static Layouts.Layout AddEntityStatus(
        this Layouts.Layout layout, BuilderFunc<Builder> builderFunc, out Disposer disposer)
    {
        var builder = new Builder();
        builderFunc(builder);
        var details = new ControlContainer { new BackgroundBox() };
        layout
            .DockFixedSizeToLeft(builder.Build(details, out disposer), Constants.UI.Menu.Width)
            .DockFixedSizeToLeft(details, Constants.UI.Menu.Width);
        return layout;
    }

    public sealed class Builder
    {
        private Binding<string>? entityName;
        private VoidEventHandler? onCloseHandler;
        private Binding<IReportSubject>? reportSubject;
        private IReportControlFactory? reportFactory;
        private readonly List<(string, Binding<string>, Binding<Color>?)> textAttributes = new();

        public Builder WithName(Binding<string> name)
        {
            entityName = name;
            return this;
        }

        public Builder WithCloseAction(VoidEventHandler onClose)
        {
            onCloseHandler = onClose;
            return this;
        }

        public Builder WithReports(Binding<IReportSubject> reports, IReportControlFactory factory)
        {
            reportSubject = reports;
            reportFactory = factory;
            return this;
        }

        public Builder AddTextAttribute(string description, Binding<string> value, Binding<Color>? color = null)
        {
            textAttributes.Add((description, value, color));
            return this;
        }

        public Control Build(ControlContainer detailsContainer, out Disposer disposer)
        {
            if (entityName == null)
            {
                throw new InvalidOperationException("Name must be set on each entity status.");
            }
            if (onCloseHandler == null)
            {
                throw new InvalidOperationException("Name must be set on each entity status.");
            }

            var control = new CompositeControl
            {
                new BackgroundBox()
            };
            var layout = control.BuildLayout()
                .ForContentBox()
                .DockFixedSizeToTop(TextFactories.Header(entityName!), Constants.UI.Text.HeaderLineHeight)
                .DockFixedSizeToBottom(
                    ButtonFactories.Button(b => b.WithLabel("Close").WithOnClick(onCloseHandler)),
                    Constants.UI.Button.Height);

            if (textAttributes.Count > 0)
            {
                layout.DockFixedSizeToTop(buildTextAttributes(out var height), height);
            }

            disposer = new Disposer();
            if (reportSubject != null)
            {
                layout.FillContent(buildReports(detailsContainer, disposer));
            }

            return control;
        }

        private Control buildTextAttributes(out double height)
        {
            var control = new CompositeControl();
            var column = control.BuildFixedColumn();
            foreach (var (description, value, color) in textAttributes)
            {
                column.AddValueLabel(description, value, rightColor: color);
            }

            height = column.Height;
            return control;
        }

        private Control buildReports(ControlContainer detailsContainer, Disposer disposer)
        {
            var control = new CompositeControl();
            reportSubject!.SourceUpdated += updateReports;
            if (reportSubject.Value != null)
            {
                updateReports(reportSubject.Value);
            }
            return control;

            void updateReports(IReportSubject subject)
            {
                control.RemoveAllChildren();
                disposer.DisposeAndReset();

                void onReportsUpdated() => updateReports(subject);
                subject.ReportsUpdated += onReportsUpdated;
                disposer.AddOnDispose(() => subject.ReportsUpdated -= onReportsUpdated);

                var column = control.BuildScrollableColumn();
                foreach (var r in subject.Reports)
                {
                    column.Add(
                        reportFactory!.CreateForReport(r, disposer, detailsContainer, out var height), height);
                }
            }
        }
    }
}
