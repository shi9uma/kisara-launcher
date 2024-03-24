<template>
    <div>
        <input v-model.number="number1" type="number" placeholder="Enter number 1">
        <input v-model.number="number2" type="number" placeholder="Enter number 2">
        <button @click="calculateSum">Calculate Sum</button>
        <div v-if="sumResult !== null">Sum: {{ sumResult }}</div>
    </div>
</template>

<script>
export default {
    data() {
        return {
            number1: 0,
            number2: 0,
            sumResult: null
        };
    },
    methods: {
        calculateSum() {
            this.sumResult = this.number1 + this.number2;
        }
    }
};
</script>