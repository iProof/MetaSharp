﻿using DevExpress.Mvvm.POCO;
using MetaSharp.Native;
using MetaSharp.Test.Meta.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xunit;

namespace MetaSharp.Test.Functional {
    public class POCOViewModelsTests {
        #region property changed
        [Fact]
        public void OverridingPropertyTest() {
            Assert.True(typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(POCOViewModel)));
            Assert.False(typeof(IPOCOViewModel).IsAssignableFrom(typeof(POCOViewModel)));
            var viewModel = POCOViewModel.Create();
            Assert.Equal(viewModel.GetType(), viewModel.GetType().GetProperty("Property1").DeclaringType);

            CheckBindableProperty(viewModel, x => x.Property1, (vm, x) => vm.Property1 = x, "x", "y");
            CheckBindableProperty(viewModel, x => x.Property2, (vm, x) => vm.Property2 = x, "m", "n");
            CheckBindableProperty(viewModel, x => x.Property3, (vm, x) => vm.Property3 = x, "a", "b");
            CheckBindableProperty(viewModel, x => x.Property4, (vm, x) => vm.Property4 = x, 1, 2);
            CheckBindableProperty(viewModel, x => x.Property5, (vm, x) => vm.Property5 = x, new Point(1, 1), new Point(2, 2));
            CheckBindableProperty(viewModel, x => x.Property6, (vm, x) => vm.Property6 = x, 5, null);
            CheckBindableProperty(viewModel, x => x.ProtectedSetterProperty, (vm, x) => vm.SetProtectedSetterProperty(x), "x", "y");
            Assert.Null(viewModel.GetType().GetProperty("ProtectedSetterProperty").GetSetMethod());
            CheckBindableProperty(viewModel, x => x.ProtectedInternalSetterProperty, (vm, x) => vm.ProtectedInternalSetterProperty = x, "x", "y");
            Assert.Null(viewModel.GetType().GetProperty("ProtectedInternalSetterProperty").GetSetMethod());
            CheckBindableProperty(viewModel, x => x.InternalSetterProperty, (vm, x) => vm.InternalSetterProperty = x, "x", "y");

            CheckNotBindableProperty(viewModel, x => x.NotVirtualProperty, (vm, x) => vm.NotVirtualProperty = x, "x", "y");
            CheckNotBindableProperty(viewModel, x => x.NotVirtualPropertyWithPrivateSetter, (vm, x) => vm.NotVirtualPropertyWithPrivateSetter = x, "x", "y");
            CheckNotBindableProperty(viewModel, x => x.NotPublicProperty, (vm, x) => vm.NotPublicProperty = x, "x", "y");
            CheckNotBindableProperty(viewModel, x => x.ProtectedGetterProperty, (vm, x) => vm.ProtectedGetterProperty = x, "x", "y");
            CheckNotBindableProperty(viewModel, x => x.NotAutoImplementedProperty, (vm, x) => vm.NotAutoImplementedProperty = x, "x", "y");
        }

        [Fact]
        public void PropertyChangedTest() {
            POCOViewModel_PropertyChanged viewModel = POCOViewModel_PropertyChanged.Create();
            ((INotifyPropertyChanged)viewModel).PropertyChanged += (o, e) => Assert.False(viewModel.OnProtectedChangedMethodWithParamChangedCalled);
            CheckBindableProperty(viewModel, x => x.ProtectedChangedMethodWithParam, (vm, x) => vm.ProtectedChangedMethodWithParam = x, "x", "y", (x, val) => {
                Assert.True(x.OnProtectedChangedMethodWithParamChangedCalled);
                x.OnProtectedChangedMethodWithParamChangedCalled = false;
                Assert.Equal(val, x.ProtectedChangedMethodWithParamOldValue);
            });

            CheckBindableProperty(viewModel, x => x.PublicChangedMethodWithoutParam, (vm, x) => vm.PublicChangedMethodWithoutParam = x, 1, 2, (x, val) => Assert.Equal(val + 1, x.PublicChangedMethodWithoutParamOldValue));
            CheckBindableProperty(viewModel, x => x.ProtectedInternalChangedMethodWithoutParam, (vm, x) => vm.ProtectedInternalChangedMethodWithoutParam = x, 1, 2, (x, val) => Assert.Equal(val + 1, x.ProtectedInternalChangedMethodWithoutParamOldValue));
            CheckBindableProperty(viewModel, x => x.InternalChangedMethodWithoutParam, (vm, x) => vm.InternalChangedMethodWithoutParam = x, 1, 2, (x, val) => Assert.Equal(val + 1, x.InternalChangedMethodWithoutParamOldValue));
            CheckBindableProperty(viewModel, x => x.PrivateChangedMethodWithoutParam, (vm, x) => vm.PrivateChangedMethodWithoutParam = x, 1, 2, (x, val) => Assert.Equal(val + 1, x.PrivateChangedMethodWithoutParamOldValue));
        }
        #endregion

        #region subscribe in constructor
        [Fact]
        public void POCOViewModel_SubscribeInCtorTest() {
            var viewModel = POCOViewModel_SubscribeInCtor.Create();
            Assert.Equal(1, viewModel.propertyChangedCallCount);
        }
        #endregion

        #region property changing
        [Fact]
        public void PropertyChangingTest() {
            var viewModel = POCOViewModel_PropertyChanging.Create();
            viewModel.Property1 = null;
            viewModel.Property1 = "x";
            Assert.Equal("x", viewModel.Property1NewValue);

            viewModel.Property2 = null;
            viewModel.Property2 = "x";
            Assert.Equal(1, viewModel.Property2ChangingCallCount);

            viewModel.Property3 = "x";
        }
        #endregion

        #region metadata
        [Fact]
        public void OverridingPropertyTest_Metadata() {
            var viewModel = POCOViewModel_WithMetadata.Create();
            CheckNotBindableProperty(viewModel, x => x.NotBindableProperty, (vm, x) => vm.NotBindableProperty = x, "x", "y");
            CheckBindableProperty(viewModel, x => x.NotAutoImplementedProperty, (vm, x) => vm.NotAutoImplementedProperty = x, "x", "y");
            CheckBindableProperty(viewModel, x => x.CustomProperytChanged, (vm, x) => vm.CustomProperytChanged = x, "x", "y", (x, val) => Assert.Equal(val, x.CustomProperytChangedOldValue));

            viewModel.PropertyChanging = null;
            viewModel.PropertyChanging = "x";
            Assert.Equal("x", viewModel.PropertyChangingNewValue);
        }
        #endregion

        #region RaisePropertyChanged implementation
        [Fact]
        public void RaisePropertyChangedImplementation() {
            var viewModel = POCOViewModel.Create();
            string propertyName = null;
            ((INotifyPropertyChanged)viewModel).PropertyChanged += (o, e) => propertyName = e.PropertyName;
            viewModel.RaisePropertyChangedInternal(x => x.Property1);
            Assert.Equal("Property1", propertyName);

            viewModel.RaisePropertyChangedInternal(x => x.Property5);
            Assert.Equal("Property5", propertyName);

            viewModel.RaisePropertiesChanged();
            Assert.Equal(string.Empty, propertyName);
        }
        #endregion

        void CheckBindableProperty<T, TProperty>(T viewModel, Expression<Func<T, TProperty>> propertyExpression, Action<T, TProperty> setValueAction, TProperty value1, TProperty value2, Action<T, TProperty> checkOnPropertyChangedResult = null) {
            CheckBindablePropertyCore(viewModel, propertyExpression, setValueAction, value1, value2, true, checkOnPropertyChangedResult);
        }
        void CheckNotBindableProperty<T, TProperty>(T viewModel, Expression<Func<T, TProperty>> propertyExpression, Action<T, TProperty> setValueAction, TProperty value1, TProperty value2) {
            CheckBindablePropertyCore(viewModel, propertyExpression, setValueAction, value1, value2, false, null);
        }
        void CheckBindablePropertyCore<T, TProperty>(T viewModel, Expression<Func<T, TProperty>> propertyExpression, Action<T, TProperty> setValueAction, TProperty value1, TProperty value2, bool bindable, Action<T, TProperty> checkOnPropertyChangedResult) {
            Assert.NotEqual(value1, value2);
            Func<T, TProperty> getValue = propertyExpression.Compile();

            int propertyChangedFireCount = 0;
            PropertyChangedEventHandler handler = (o, e) => {
                Assert.Equal(viewModel, o);
                Assert.Equal(ExpressionExtensions.GetPropertyNameFast(propertyExpression), e.PropertyName);
                propertyChangedFireCount++;
            };
            ((INotifyPropertyChanged)viewModel).PropertyChanged += handler;
            Assert.Equal(0, propertyChangedFireCount);
            TProperty oldValue = getValue(viewModel);
            setValueAction(viewModel, value1);
            if(checkOnPropertyChangedResult != null)
                checkOnPropertyChangedResult(viewModel, oldValue);
            if(bindable) {
                Assert.Equal(value1, getValue(viewModel));
                Assert.Equal(1, propertyChangedFireCount);
            } else {
                Assert.Equal(0, propertyChangedFireCount);
            }
            ((INotifyPropertyChanged)viewModel).PropertyChanged -= handler;
            setValueAction(viewModel, value2);
            setValueAction(viewModel, value2);
            if(checkOnPropertyChangedResult != null)
                checkOnPropertyChangedResult(viewModel, value1);
            if(bindable) {
                Assert.Equal(value2, getValue(viewModel));
                Assert.Equal(1, propertyChangedFireCount);
            } else {
                Assert.Equal(0, propertyChangedFireCount);
            }
        }

    }
}
