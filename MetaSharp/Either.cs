﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaSharp {
    public abstract class Either<TLeft, TRight> {
        public static implicit operator Either<TLeft, TRight>(TLeft val)  {
            return new LeftValue(val);
        }
        public static implicit operator Either<TLeft, TRight>(TRight val) {
            return new RightValue(val);
        }
        public static Either<TLeft, TRight> Left(TLeft value) {
            return new LeftValue(value);
        }
        public static Either<TLeft, TRight> Right(TRight value) {
            return new RightValue(value);
        }
        Either() { }
        internal class LeftValue : Either<TLeft, TRight> {
            internal readonly TLeft Value;
            internal LeftValue(TLeft value) {
                Value = value;
            }
        }
        internal class RightValue : Either<TLeft, TRight> {
            internal readonly TRight Value;
            internal RightValue(TRight value) {
                Value = value;
            }
        }
    }
    public static class Either {
        //TODO use friend access diagnostics - only this class can access Either's internals
        public static bool IsRight<TLeft, TRight>(this Either<TLeft, TRight> value) {
            return value.Match(left => false, right => true);
        }
        public static bool IsLeft<TLeft, TRight>(this Either<TLeft, TRight> value) {
            return value.Match(left => true, right => false);
        }
        public static TLeft ToLeft<TLeft, TRight>(this Either<TLeft, TRight> value) {
            return value.Match(left => left, right => { throw new InvalidOperationException(); });
        }
        public static TRight ToRight<TLeft, TRight>(this Either<TLeft, TRight> value) {
            return value.Match(left => { throw new InvalidOperationException(); }, right => right);
        }
        public static T Match<TLeft, TRight, T>(this Either<TLeft, TRight> value, Func<TLeft, T> left, Func<TRight, T> right) {
            var leftValue = value as Either<TLeft, TRight>.LeftValue;
            if(leftValue != null)
                return left(leftValue.Value);
            return right((value as Either<TLeft, TRight>.RightValue).Value);
        }
        public static void Match<TLeft, TRight>(this Either<TLeft, TRight> value, Action<TLeft> left, Action<TRight> right) {
            var leftValue = value as Either<TLeft, TRight>.LeftValue;
            if(leftValue != null)
                left(leftValue.Value);
            right((value as Either<TLeft, TRight>.RightValue).Value);
        }
        public static Either<TLeft, TRightNew> Select<TLeft, TRight, TRightNew>(this Either<TLeft, TRight> value, Func<TRight, TRightNew> selector) {
            return value.Match(
                left => Either<TLeft, TRightNew>.Left(left),
                right => Either<TLeft, TRightNew>.Right(selector(right))
            );
        }
        //public static Either<TLeft, TRight> Where<TLeft, TRight>(this Either<TLeft, TRight> value, Predicate<TRight> predicate) {
        //    return value.Match(
        //        left => Either<TLeft, TRightNew>.Left(left),
        //        right => Either<TLeft, TRightNew>.Right(selector(right))
        //    );
        //}
        public static Either<TLeftNew, TRightNew> Transform<TLeft, TRight, TLeftNew, TRightNew>(this Either<TLeft, TRight> value, Func<TLeft, TLeftNew> selectorLeft, Func<TRight, TRightNew> selectorRight) {
            return value.Match(
                left => Either<TLeftNew, TRightNew>.Left(selectorLeft(left)),
                right => Either<TLeftNew, TRightNew>.Right(selectorRight(right))
            );
        }
        public static Either<TLeftAcc, TRightAcc> AggregateEither<TLeft, TRight, TLeftAcc, TRightAcc>(
            this IEnumerable<Either<TLeft, TRight>> source,
            TLeftAcc leftSeed,
            TRightAcc rightSeed,
            Func<TLeftAcc, TLeft, TLeftAcc> leftAcc,
            Func<TRightAcc, TRight, TRightAcc> rightAcc) {
            return source.Aggregate(
                Either<TLeftAcc, TRightAcc>.Right(rightSeed),
                (acc, value) => acc.Match(
                    accLeft => Either<TLeftAcc, TRightAcc>.Left(value.Match(left => leftAcc(accLeft, left), right => accLeft)),
                    accRight => value.Match(left => Either<TLeftAcc, TRightAcc>.Left(leftAcc(leftSeed, left)), right => Either<TLeftAcc, TRightAcc>.Right(rightAcc(accRight, right)))
                )
            );
        }
    }
}