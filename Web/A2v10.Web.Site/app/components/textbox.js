﻿// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

/*20180729-7259*/
/*components/textbox.js*/

(function () {

	const utils = require('std:utils');
	const mask = require('std:mask');

	let textBoxTemplate =
		`<div :class="cssClass()">
	<label v-if="hasLabel" v-text="label" />
	<div class="input-group">
		<input ref="input" :type="controlType" v-focus autocomplete="off" :id="testId"
			v-bind:value="modelValue" 
					v-on:change="onChange($event.target.value)" 
					v-on:input="onInput($event.target.value)"
				:class="inputClass" :placeholder="placeholder" :disabled="disabled" :tabindex="tabIndex" :maxlength="maxLength"/>
		<slot></slot>
		<validator :invalid="invalid" :errors="errors" :options="validatorOptions"></validator>
	</div>
	<slot name="popover"></slot>
	<span class="descr" v-if="hasDescr" v-text="description"></span>
</div>
`;

	let textAreaTemplate =
		`<div :class="cssClass()">
	<label v-if="hasLabel" v-text="label" />
	<div class="input-group">
		<textarea v-focus v-auto-size="autoSize" v-bind:value="modelValue2" :id="testId"
			v-on:change="onChange($event.target.value)" 
			v-on:input="onInput($event.target.value)"
			:rows="rows" :class="inputClass" :placeholder="placeholder" :disabled="disabled" :tabindex="tabIndex" :maxlength="maxLength"/>
		<slot></slot>
		<validator :invalid="invalid" :errors="errors" :options="validatorOptions"></validator>
	</div>
	<slot name="popover"></slot>
	<span class="descr" v-if="hasDescr" v-text="description"></span>
</div>
`;

	let staticTemplate =
		`<div :class="cssClass()">
	<label v-if="hasLabel" v-text="label" />
	<div class="input-group static">
		<span v-focus v-text="textProp" :class="inputClass" :tabindex="tabIndex" :id="testId"/>
		<slot></slot>
		<validator :invalid="invalid" :errors="errors" :options="validatorOptions"></validator>
	</div>
	<slot name="popover"></slot>
	<span class="descr" v-if="hasDescr" v-text="description"></span>
</div>
`;

	/*
	<span>{{ path }}</span>
		<button @click="test" >*</button >
	*/

	let baseControl = component('control');

	Vue.component('textbox', {
		extends: baseControl,
		template: textBoxTemplate,
		props: {
			item: {
				type: Object, default() {
					return {};
				}
			},
			prop: String,
			itemToValidate: Object,
			propToValidate: String,
			placeholder: String,
			password: Boolean
		},
		computed: {
			controlType() {
				return this.password ? "password" : "text";
			}
		},
		methods: {
			updateValue(value) {
				if (this.mask)
					this.item[this.prop] = mask.getUnmasked(this.mask, value);
				else
					this.item[this.prop] = utils.parse(value, this.dataType);
				if (this.$refs.input.value !== this.modelValue) {
					this.$refs.input.value = this.modelValue;
					this.$emit('change', this.item[this.prop]);
				}
			},
			onInput(value) {
				if (this.updateTrigger === 'input')
					this.updateValue(value);
			},
			onChange(value) {
				if (this.updateTrigger !== 'input')
					this.updateValue(value);
			}
		}
	});

	Vue.component('a2-textarea', {
		extends: baseControl,
		template: textAreaTemplate,
		props: {
			item: {
				type: Object, default() {
					return {};
				}
			},
			prop: String,
			itemToValidate: Object,
			propToValidate: String,
			placeholder: String,
			autoSize: Boolean,
			rows: Number
		},
		computed: {
			modelValue2() {
				if (!this.item) return null;
				return this.item[this.prop];
			}
		},
		methods: {
			updateValue(value) {
				if (this.item[this.prop] === value) return;
				this.item[this.prop] = value;
				this.$emit('change', this.item[this.prop]);
			},
			onInput(value) {
				if (this.updateTrigger === 'input')
					this.updateValue(value);
			},
			onChange(value) {
				if (this.updateTrigger !== 'input')
					this.updateValue(value);
			}
		}
	});

	Vue.component('static', {
		extends: baseControl,
		template: staticTemplate,
		props: {
			item: {
				type: Object, default() {
					return {};
				}
			},
			prop: String,
			itemToValidate: Object,
			propToValidate: String,
			text: [String, Number, Date]
		}, 
		computed: {
			textProp() {
				if (this.mask && this.text)
					return mask.getMasked(this.mask, this.text);
				return this.text;
			}
		}
	});

})();