﻿using System.Collections.Generic;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Examine;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Configures and installs Examine.
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ExamineComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            // populators are not a collection: once cannot remove ours, and can only add more
            // the container can inject IEnumerable<IIndexPopulator> and get them all
            composition.Register<IIndexPopulator, MemberIndexPopulator>(Lifetime.Singleton);
            composition.Register<IIndexPopulator, ContentIndexPopulator>(Lifetime.Singleton);
            composition.Register<IIndexPopulator, PublishedContentIndexPopulator>(Lifetime.Singleton);
            composition.Register<IIndexPopulator, MediaIndexPopulator>(Lifetime.Singleton);

            composition.Register<IndexRebuilder>(Lifetime.Singleton);
            composition.RegisterUnique<IUmbracoIndexesCreator, UmbracoIndexesCreator>();
            composition.RegisterUnique<IPublishedContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetInstance<PropertyEditorCollection>(),
                    factory.GetInstance<IEnumerable<IUrlSegmentProvider>>(),
                    factory.GetInstance<IUserService>(),
                    true));
            composition.RegisterUnique<IContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetInstance<PropertyEditorCollection>(),
                    factory.GetInstance<IEnumerable<IUrlSegmentProvider>>(),
                    factory.GetInstance<IUserService>(),
                    false));
            composition.RegisterUnique<IValueSetBuilder<IMedia>, MediaValueSetBuilder>();
            composition.RegisterUnique<IValueSetBuilder<IMember>, MemberValueSetBuilder>();

            //We want to manage Examine's appdomain shutdown sequence ourselves so first we'll disable Examine's default behavior
            //and then we'll use MainDom to control Examine's shutdown - this MUST be done in Compose ie before ExamineManager
            //is instantiated, as the value is used during instantiation
            ExamineManager.DisableDefaultHostingEnvironmentRegistration();

            composition.Components().Append<ExamineComponent>();
        }
    }
}