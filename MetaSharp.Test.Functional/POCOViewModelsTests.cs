﻿using MetaSharp.Native;
using MetaSharp.Test.Meta.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MetaSharp.Test.Functional {
    public class POCOViewModelsTests {

        [Fact]
        public void OverridingPropertyTest() {
            var viewModel = POCOViewModel.Create();
            //Assert.AreEqual(viewModel.GetType(), viewModel.GetType().GetProperty("Property1").DeclaringType);

            //CheckBindableProperty(viewModel, x => x.Property1, (vm, x) => vm.Property1 = x, "x", "y");
            //CheckBindableProperty(viewModel, x => x.Property2, (vm, x) => vm.Property2 = x, "m", "n");
            //CheckBindableProperty(viewModel, x => x.Property3, (vm, x) => vm.Property3 = x, "a", "b");
            //CheckBindableProperty(viewModel, x => x.Property4, (vm, x) => vm.Property4 = x, 1, 2);
            //CheckBindableProperty(viewModel, x => x.Property5, (vm, x) => vm.Property5 = x, new Point(1, 1), new Point(2, 2));
            //CheckBindableProperty(viewModel, x => x.Property6, (vm, x) => vm.Property6 = x, 5, null);
            //CheckBindableProperty(viewModel, x => x.ProtectedSetterProperty, (vm, x) => vm.ProtectedSetterProperty = x, "x", "y");
            //Assert.IsNull(viewModel.GetType().GetProperty("ProtectedSetterProperty").GetSetMethod());

            //CheckNotBindableProperty(viewModel, x => x.NotVirtualProperty, (vm, x) => vm.NotVirtualProperty = x, "x", "y");
            //CheckNotBindableProperty(viewModel, x => x.NotPublicProperty, (vm, x) => vm.NotPublicProperty = x, "x", "y");
            //CheckNotBindableProperty(viewModel, x => x.ProtectedGetterProperty, (vm, x) => vm.ProtectedGetterProperty = x, "x", "y");
            //CheckNotBindableProperty(viewModel, x => x.InternalSetterProperty, (vm, x) => vm.InternalSetterProperty = x, "x", "y");
            //CheckNotBindableProperty(viewModel, x => x.NotAutoImplementedProperty, (vm, x) => vm.NotAutoImplementedProperty = x, "x", "y");
        }

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
